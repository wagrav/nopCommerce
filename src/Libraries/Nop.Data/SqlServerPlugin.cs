using System;
using System.Data.SqlClient;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nop.Data
{
    public class SqlServerPlugin : IDbPlugin
    {
        #region Utilities

        /// <summary>
        /// Create contents of connection strings used by the NpgsqlConnection class
        /// </summary>
        /// <param name="trustedConnection">A value that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true)</param>
        /// <param name="serverName">The name or network address of the instance of Ms Sql Server to connect to</param>
        /// <param name="port">The server port</param>
        /// <param name="databaseName">The name of the database associated with the connection</param>
        /// <param name="userName">The user ID to be used when connecting to Ms Sql Server</param>
        /// <param name="password">The password for the Ms Sql Server account</param>
        /// <param name="timeout">The connection timeout</param>
        /// <returns>Connection string</returns>
        protected string CreateConnectionString(bool trustedConnection,
            string serverName, int port, string databaseName,
            string userName, string password, int timeout = 0)
        {
            var builder = new SqlConnectionStringBuilder
            {
                IntegratedSecurity = trustedConnection,
                DataSource = serverName,
                InitialCatalog = databaseName
            };

            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }

            builder.PersistSecurityInfo = false;

            if (UseMars)
            {
                builder.MultipleActiveResultSets = true;
            }

            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
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
                    modelState.AddModelError(string.Empty, "ConnectionStringRequired");

                try
                {
                    //try to create connection string
                    new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                }
                catch
                {
                    modelState.AddModelError(string.Empty, "ConnectionStringWrongFormat");
                }
            }
            else
            {
                //values
                if (string.IsNullOrEmpty(model.SqlServerName))
                    modelState.AddModelError(string.Empty, "SqlServerNameRequired");
                if (string.IsNullOrEmpty(model.SqlDatabaseName))
                    modelState.AddModelError(string.Empty, "DatabaseNameRequired");

                //authentication type
                if (!model.SqlAuthenticationType.Equals("sqlauthentication", StringComparison.InvariantCultureIgnoreCase)) 
                    return;

                //SQL authentication
                if (string.IsNullOrEmpty(model.SqlServerUsername))
                    modelState.AddModelError(string.Empty, "SqlServerUsernameRequired");
                if (string.IsNullOrEmpty(model.SqlServerPassword))
                    modelState.AddModelError(string.Empty, "SqlServerPasswordRequired");
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
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                //now create connection string to 'master' database. It always exists.
                builder.InitialCatalog = "master";
                var masterCatalogConnectionString = builder.ToString();
                var query = $"CREATE DATABASE [{databaseName}]";
                if (!string.IsNullOrWhiteSpace(collation))
                    query = $"{query} COLLATE {collation}";
                using (var conn = new SqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                //try connect
                if (triesToConnect <= 0) 
                    return string.Empty;

                //Sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
                //But we have already started creation of tables and sample data.
                //As a result there is an exception thrown and the installation process cannot continue.
                //That's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
                for (var i = 0; i <= triesToConnect; i++)
                {
                    if (i == triesToConnect)
                        throw new Exception("Unable to connect to the new database. Please try one more time");

                    if (!DatabaseExists(connectionString))
                        Thread.Sleep(1000);
                    else
                        break;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Format("DatabaseCreationError", ex.Message);
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
                using (var conn = new SqlConnection(connectionString))
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
            return "~/Views/Install/_MsSqlDbProvider.cshtml";
        }

        /// <summary>
        /// Gets a DbConnectionInfo view URL
        /// </summary>
        public string DbConnectionInfo()
        {
            return "~/Views/Install/_MsSqlConnectionInfo.cshtml";
        }

        /// <summary>
        /// Create connection strings
        /// </summary>
        /// <param name="model">Installation model</param>
        /// <returns>Connection string</returns>
        public string GetConnectionString(IDbPluginInstallModel model)
        {
            string connectionString;

            if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
            {
                //raw connection string
                var sqlCsb = new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                connectionString = sqlCsb.ToString();
            }
            else
            {
                //values
                connectionString = CreateConnectionString(model.SqlAuthenticationType == "windowsauthentication",
                    model.SqlServerName, model.SqlServerPort, model.SqlDatabaseName,
                    model.SqlServerUsername, model.SqlServerPassword);
            }

            return connectionString;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Reutuns provider name
        /// </summary>
        public string DataProviderName => NopDataDefaults.DataProviderName;

        /// <summary>
        /// A value indicating whether we use MARS (Multiple Active Result Sets)
        /// </summary>
        protected virtual bool UseMars => false;

        #endregion
    }
}