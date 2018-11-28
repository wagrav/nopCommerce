using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Data;
using Nop.Services.Installation;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Security;
using Nop.Web.Infrastructure.Installation;
using Nop.Web.Models.Install;

namespace Nop.Web.Controllers
{
    public partial class InstallController : Controller
    {
        #region Fields

        private static Dictionary<string, Type> _providerTypes;
        private static List<IDbPlugin> _dbPlugins;

        private readonly IEngine _engine;
        private readonly IInstallationLocalizationService _locService;
        private readonly INopFileProvider _fileProvider;
        private readonly NopConfig _config;

        #endregion

        #region Ctor

        public InstallController(
            IEngine engine,
            IInstallationLocalizationService locService,
            INopFileProvider fileProvider,
            NopConfig config)
        {
            this._engine = engine;
            this._locService = locService;
            this._fileProvider = fileProvider;
            this._config = config;
        }

        #endregion

        
        #region Methods

        public virtual IActionResult Index()
        {
            _providerTypes = new Dictionary<string, Type>();
            _dbPlugins = new List<IDbPlugin>();

            if (DataSettingsManager.DatabaseIsInstalled)
                return RedirectToRoute("HomePage");

            var model = new InstallModel
            {
                AdminEmail = "admin@yourStore.com",
                InstallSampleData = false,
                DatabaseConnectionString = string.Empty,
                DataProvider = "SqlServer",
                //fast installation service does not support SQL compact
                DisableSampleDataOption = _config.DisableSampleDataDuringInstallation,
                SqlAuthenticationType = "sqlauthentication",
                SqlConnectionInfo = "sqlconnectioninfo_values",
                SqlServerCreateDatabase = false,
                UseCustomCollation = false,
                Collation = "SQL_Latin1_General_CP1_CI_AS"
            };
            foreach (var lang in _locService.GetAvailableLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = _locService.GetCurrentLanguage().Code == lang.Code
                });
            }

            var typeFinder = new WebAppTypeFinder();
            var bdPluginsTypes = typeFinder.FindClassesOfType<IDbPlugin>().ToList();
            foreach (var bdPluginType in bdPluginsTypes)
            {
                if (!(_engine.ResolveUnregistered(bdPluginType) is IDbPlugin bdPlugin))
                    continue;

                _dbPlugins.Add(bdPlugin);
                _providerTypes.Add(bdPlugin.DataProviderName, bdPlugin.GetType());
            }

            model.DbPlugins = _dbPlugins;

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Index(InstallModel model)
        {
            if (DataSettingsManager.DatabaseIsInstalled)
                return RedirectToRoute("HomePage");

            if (model.DatabaseConnectionString != null)
                model.DatabaseConnectionString = model.DatabaseConnectionString.Trim();
            
            var bdPlugin = _engine.ResolveUnregistered(_providerTypes[model.DataProvider]) as IDbPlugin;

            if (bdPlugin == null)
                throw new ArgumentNullException(nameof(bdPlugin));

            bdPlugin.CheckModel(model, ModelState);

            //prepare language list
            foreach (var lang in _locService.GetAvailableLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = _locService.GetCurrentLanguage().Code == lang.Code
                });
            }

            model.DisableSampleDataOption = _config.DisableSampleDataDuringInstallation;
            
            //Consider granting access rights to the resource to the ASP.NET request identity. 
            //ASP.NET has a base process identity 
            //(typically {MACHINE}\ASPNET on IIS 5 or Network Service on IIS 6 and IIS 7, 
            //and the configured application pool identity on IIS 7.5) that is used if the application is not impersonating.
            //If the application is impersonating via <identity impersonate="true"/>, 
            //the identity will be the anonymous user (typically IUSR_MACHINENAME) or the authenticated request user.
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            //validate permissions
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite();
            foreach (var dir in dirsToCheck)
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                    ModelState.AddModelError(string.Empty, string.Format(_locService.GetResource("ConfigureDirectoryPermissions"), WindowsIdentity.GetCurrent().Name, dir));

            var filesToCheck = FilePermissionHelper.GetFilesWrite();
            foreach (var file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                    ModelState.AddModelError(string.Empty, string.Format(_locService.GetResource("ConfigureFilePermissions"), WindowsIdentity.GetCurrent().Name, file));

            if (ModelState.IsValid)
            {
                try
                {
                    var connectionString = bdPlugin.GetConnectionString(model);

                    if (model.SqlServerCreateDatabase)
                    {
                        if (!bdPlugin.DatabaseExists(connectionString))
                        {
                            //create database
                            var collation = model.UseCustomCollation ? model.Collation : string.Empty;
                            var errorCreatingDatabase = bdPlugin.CreateDatabase(connectionString, collation);
                            if (!string.IsNullOrEmpty(errorCreatingDatabase))
                                throw new Exception(errorCreatingDatabase);
                        }
                    }
                    else
                    {
                        //check whether database exists
                        if (!bdPlugin.DatabaseExists(connectionString))
                            throw new Exception(_locService.GetResource("DatabaseNotExists"));
                    }

                    DataSettingsManager.SaveSettings(new DataSettings
                    {
                        DataProvider = bdPlugin.DataProviderName,
                        DataConnectionString = connectionString
                    }, _fileProvider);

                    //initialize database
                    EngineContext.Current.Resolve<IDataProvider>().InitializeDatabase();

                    //now resolve installation service
                    var installationService = EngineContext.Current.Resolve<IInstallationService>();
                    installationService.InstallData(model.AdminEmail.ToLower(), model.AdminPassword, model.InstallSampleData);

                    //reset cache
                    DataSettingsManager.ResetCache();

                    //add plugins to install list
                    PluginManager.PluginsInfo.MarkAllPluginsAsUninstalled();
                    var pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
                    var plugins = pluginFinder.GetPluginDescriptors(LoadPluginsMode.All)
                        .ToList()
                        .OrderBy(x => x.Group)
                        .ThenBy(x => x.DisplayOrder)
                        .ToList();
                    var pluginsIgnoredDuringInstallation = string.IsNullOrEmpty(_config.PluginsIgnoredDuringInstallation) ?
                        new List<string>() :
                        _config.PluginsIgnoredDuringInstallation
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();
                    foreach (var plugin in plugins)
                    {
                        if (pluginsIgnoredDuringInstallation.Contains(plugin.SystemName))
                            continue;

                        PluginManager.PluginsInfo.AddToInstall(plugin.SystemName);
                    }

                    //register default permissions
                    //var permissionProviders = EngineContext.Current.Resolve<ITypeFinder>().FindClassesOfType<IPermissionProvider>();
                    var permissionProviders = new List<Type> { typeof(StandardPermissionProvider) };
                    foreach (var providerType in permissionProviders)
                    {
                        var provider = (IPermissionProvider)Activator.CreateInstance(providerType);
                        EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);
                    }

                    //restart application
                    webHelper.RestartAppDomain();

                    //Redirect to home page
                    return RedirectToRoute("HomePage");
                }
                catch (Exception exception)
                {
                    //reset cache
                    DataSettingsManager.ResetCache();

                    var cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                    cacheManager.Clear();

                    //clear provider settings if something got wrong
                    DataSettingsManager.SaveSettings(new DataSettings(), _fileProvider);

                    ModelState.AddModelError(string.Empty, string.Format(_locService.GetResource("SetupFailed"), exception.Message));
                }
            }

            model.DbPlugins = _dbPlugins;

            return View(model);
        }

        public virtual IActionResult ChangeLanguage(string language)
        {
            if (DataSettingsManager.DatabaseIsInstalled)
                return RedirectToRoute("HomePage");

            _locService.SaveCurrentLanguage(language);

            //Reload the page
            return RedirectToAction("Index", "Install");
        }

        [HttpPost]
        public virtual IActionResult RestartInstall()
        {
            if (DataSettingsManager.DatabaseIsInstalled)
                return RedirectToRoute("HomePage");

            //restart application
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            webHelper.RestartAppDomain();

            //Redirect to home page
            return RedirectToRoute("HomePage");
        }

        #endregion
    }
}