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
        public string DataProviderName => PluginName;

        public static string PluginName => typeof(PostgreSQLDataProvider).Name;

        private readonly IInstallationLocalizationService _locService;

        public PostgreSQLPlugin(IInstallationLocalizationService locService)
        {
            this._locService = locService;
        }


        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return string.Empty;
            //return _webHelper.GetStoreLocation() + "Admin/WidgetsNivoSlider/Configure";
        }

        public string DbProvider()
        {
            return "~/Plugins/Data.PostgreSQL/Views/_DbProvider.cshtml";
        }

        public string DbConnectionInfo()
        {
            return "~/Plugins/Data.PostgreSQL/Views/_DbConnectionInfo.cshtml";
        }

        public string GetConnectionString(IDbPluginInstallModel model)
        {
            var connectionString = string.Empty;
            if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
            {
                //raw connection string
                var sqlCsb = new NpgsqlConnectionStringBuilder(model.DatabaseConnectionString);
                connectionString = sqlCsb.ToString();
                //tableName = sqlCsb.Database;
            }
            else
            {
                //values
                connectionString = CreateConnectionString(false,
                    model.SqlServerName, model.SqlServerPort, model.SqlDatabaseName,
                    model.SqlServerUsername, model.SqlServerPassword);


                //tableName = model.SqlDatabaseName;
            }
            return connectionString;

        }

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
        /// Creates a database on the MS SQL server.
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
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            //var sampleImagesPath = _fileProvider.MapPath("~/Plugins/Widgets.NivoSlider/Content/nivoslider/sample-images/");

            //settings
            //var settings = new NivoSliderSettings
            //{
            //    Picture1Id = _pictureService.InsertPicture(_fileProvider.ReadAllBytes(sampleImagesPath + "banner1.jpg"), MimeTypes.ImagePJpeg, "banner_1").Id,
            //    Text1 = "",
            //    Link1 = _webHelper.GetStoreLocation(false),
            //    Picture2Id = _pictureService.InsertPicture(_fileProvider.ReadAllBytes(sampleImagesPath + "banner2.jpg"), MimeTypes.ImagePJpeg, "banner_2").Id,
            //    Text2 = "",
            //    Link2 = _webHelper.GetStoreLocation(false)
            //    //Picture3Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner3.jpg"), MimeTypes.ImagePJpeg, "banner_3").Id,
            //    //Text3 = "",
            //    //Link3 = _webHelper.GetStoreLocation(false),
            //};
            //_settingService.SaveSetting(settings);


            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture1", "Picture 1");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture2", "Picture 2");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture3", "Picture 3");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture4", "Picture 4");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture5", "Picture 5");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture", "Picture");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture.Hint", "Upload picture.");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Text", "Comment");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Text.Hint", "Enter comment for picture. Leave empty if you don't want to display any text.");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Link", "URL");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Link.Hint", "Enter URL. Leave empty if you don't want this picture to be clickable.");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText", "Image alternate text");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText.Hint", "Enter alternate text that will be added to image.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            //_settingService.DeleteSetting<NivoSliderSettings>();

            //locales
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture1");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture2");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture3");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture4");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture5");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture.Hint");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Text");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Text.Hint");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Link");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Link.Hint");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText.Hint");

            base.Uninstall();
        }




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
    }
}