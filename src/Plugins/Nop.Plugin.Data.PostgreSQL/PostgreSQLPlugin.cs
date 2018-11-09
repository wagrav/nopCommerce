using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Data;
using Nop.Plugin.Data.PostgreSQL.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Infrastructure.Installation;
using Npgsql;

namespace Nop.Plugin.Data.PostgreSQL
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class PostgreSQLPlugin : BasePlugin, IDbPlugin
    {
        #region Fields

        private readonly IInstallationLocalizationService _locService;

        #endregion

        #region Ctor

        public PostgreSQLPlugin(IInstallationLocalizationService locService)
        {
            this._locService = locService;
        }

        #endregion


        #region Utilities
        /// <summary>
        /// Create contents of connection strings used by the NpgsqlConnection class
        /// </summary>
        /// <param name="trustedConnection">Avalue that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true)</param>
        /// <param name="serverName">The name or network address of the instance of PostgreSQL Server to connect to</param>
        /// <param name="databaseName">The name of the database associated with the connection</param>
        /// <param name="userName">The user ID to be used when connecting to PosgreSQL Server</param>
        /// <param name="password">The password for the PosgreSQL Server account</param>
        /// <param name="timeout">The connection timeout</param>
        /// <returns>Connection string</returns>
        protected string CreateConnectionString(bool trustedConnection,
            string serverName, int port, string databaseName,
            string userName, string password, int timeout = 0)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = serverName,
                Database = databaseName,
                Port = port
            };

            if (!trustedConnection)
            {
                builder.Username = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;

            if (timeout > 0)
            {
                builder.Timeout = timeout;
            }
            return builder.ConnectionString;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Check install
        /// </summary>
        /// <param name="model">Installation model</param>
        /// <param name="modelState"> State of model</param>
        public void CheckModel(IDbPluginInstallModel model, ModelStateDictionary modelState)
        {
            if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
            {
                //raw connection string
                if (string.IsNullOrEmpty(model.DatabaseConnectionString))
                    modelState.AddModelError("", _locService.GetResource("ConnectionStringRequired"));

                try
                {
                    //try to create connection string
                    new NpgsqlConnectionStringBuilder(model.DatabaseConnectionString);
                }
                catch
                {
                    modelState.AddModelError("", _locService.GetResource("ConnectionStringWrongFormat"));
                }
            }
            else
            {
                //values
                if (string.IsNullOrEmpty(model.SqlServerName))
                    modelState.AddModelError("", _locService.GetResource("SqlServerNameRequired"));
                if (string.IsNullOrEmpty(model.SqlDatabaseName))
                    modelState.AddModelError("", _locService.GetResource("DatabaseNameRequired"));

                //authentication type
                if (model.SqlAuthenticationType.Equals("sqlauthentication", StringComparison.InvariantCultureIgnoreCase))
                {
                    //SQL authentication
                    if (string.IsNullOrEmpty(model.SqlServerUsername))
                        modelState.AddModelError("", _locService.GetResource("SqlServerUsernameRequired"));
                    if (string.IsNullOrEmpty(model.SqlServerPassword))
                        modelState.AddModelError("", _locService.GetResource("SqlServerPasswordRequired"));
                }
            }
        }

        /// <summary>
        /// Creates a database on the PosgreSQL server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="collation">Server collation; the default one will be used if not specified</param>
        /// <param name="triesToConnect">
        /// Number of times to try to connect to database. 
        /// If connection cannot be open, then error will be returned. 
        /// Pass 0 to skip this validation.
        /// </param>
        /// <returns>Error</returns>
        public string CreateDatabase(string connectionString, string collation, int triesToConnect = 10)
        {
            try
            {
                //parse database name
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                var databaseName = builder.Database;
                //now create connection string to 'postgres' dabatase. It always exists.
                builder.Database = "postgres";
                var masterCatalogConnectionString = builder.ToString();
                var query = $"CREATE DATABASE {databaseName};";
                if (!string.IsNullOrWhiteSpace(collation))
                    query = $"{query} COLLATE {collation}";
                using (var conn = new NpgsqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                //try connect
                if (triesToConnect > 0)
                {
                    //Sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
                    //But we have already started creation of tables and sample data.
                    //As a result there is an exception thrown and the installation process cannot continue.
                    //That's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
                    for (var i = 0; i <= triesToConnect; i++)
                    {
                        if (i == triesToConnect)
                            throw new Exception("Unable to connect to the new database. Please try one more time");

                        if (!this.DatabaseExists(connectionString))
                            Thread.Sleep(1000);
                        else
                            break;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Format(_locService.GetResource("DatabaseCreationError"), ex.Message);
            }
        }

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        public bool DatabaseExists(string connectionString)
        {
            try
            {
                //just try to connect
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a DbProvider view URL
        /// </summary>
        public string DbProvider()
        {
            return "~/Plugins/Data.PostgreSQL/Views/_DbProvider.cshtml";
        }

        /// <summary>
        /// Gets a DbConnectionInfo view URL
        /// </summary>
        public string DbConnectionInfo()
        {
            return "~/Plugins/Data.PostgreSQL/Views/_DbConnectionInfo.cshtml";
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return string.Empty;
            //return _webHelper.GetStoreLocation() + "Admin/WidgetsNivoSlider/Configure";
        }

        /// <summary>
        /// Create connection strings
        /// </summary>
        /// <param name="model">Installation model</param>
        /// <returns>Connection string</returns>
        public string GetConnectionString(IDbPluginInstallModel model)
        {
            var connectionString = string.Empty;
            if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
            {
                //raw connection string
                var sqlCsb = new NpgsqlConnectionStringBuilder(model.DatabaseConnectionString);
                connectionString = sqlCsb.ToString();
            }
            else
            {
                //values
                connectionString = CreateConnectionString(false,
                    model.SqlServerName, model.SqlServerPort, model.SqlDatabaseName,
                    model.SqlServerUsername, model.SqlServerPassword);
            }
            return connectionString;

        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }

        #endregion

        #region Properties

        public string DataProviderName => typeof(PostgreSQLDataProvider).Name;

        #endregion
    }
}