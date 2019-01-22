using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;

namespace Nop.Core.Plugins
{
    /// <summary>
    /// Represents an information about plugins
    /// </summary>
    public partial class PluginsInfo: IPluginsInfo
    {
        #region Fields

        protected readonly INopFileProvider _fileProvider;

        #endregion

        #region Utilities

        /// <summary>
        /// Get system names of installed plugins from obsolete file
        /// </summary>
        /// <returns>List of plugin system names</returns>
        protected virtual IList<string> GetObsoleteInstalledPluginNames()
        {
            //check whether file exists
            var filePath = _fileProvider.MapPath(NopPluginDefaults.InstalledPluginsFilePath);
            if (!_fileProvider.FileExists(filePath))
            {
                //if not, try to parse the file that was used in previous nopCommerce versions
                filePath = _fileProvider.MapPath(NopPluginDefaults.ObsoleteInstalledPluginsFilePath);
                if (!_fileProvider.FileExists(filePath))
                    return new List<string>();

                //get plugin system names from the old txt file
                var pluginSystemNames = new List<string>();
                using (var reader = new StringReader(_fileProvider.ReadAllText(filePath, Encoding.UTF8)))
                {
                    string pluginName;
                    while ((pluginName = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(pluginName))
                            pluginSystemNames.Add(pluginName.Trim());
                    }
                }

                //and delete the old one
                _fileProvider.DeleteFile(filePath);

                return pluginSystemNames;
            }

            var text = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            //delete the old file
            _fileProvider.DeleteFile(filePath);

            //get plugin system names from the JSON file
            return JsonConvert.DeserializeObject<IList<string>>(text);
        }

        /// <summary>
        /// Deserialize PluginInfo from json
        /// </summary>
        /// <param name="json">Json data of PluginInfo</param>
        protected virtual void DeserializePluginInfo(string json)
        {
            var pluginsInfo = JsonConvert.DeserializeObject<PluginsInfo>(json);

            InstalledPluginNames = pluginsInfo.InstalledPluginNames;
            PluginNamesToUninstall = pluginsInfo.PluginNamesToUninstall;
            PluginNamesToDelete = pluginsInfo.PluginNamesToDelete;
            PluginNamesToInstall = pluginsInfo.PluginNamesToInstall;
        }

        #endregion

        #region Ctor

        public PluginsInfo(INopFileProvider fileProvider)
        {
            this._fileProvider = fileProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Save plugins info to the file
        /// </summary>
        public virtual void Save()
        {
            //save the file
            var filePath = _fileProvider.MapPath(NopPluginDefaults.PluginsInfoFilePath);
            var text = JsonConvert.SerializeObject(this, Formatting.Indented);
            _fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        }

        /// <summary>
        /// Get plugins info
        /// </summary>
        public virtual void LoadPluginInfo()
        {
            //check whether plugins info file exists
            var filePath = _fileProvider.MapPath(NopPluginDefaults.PluginsInfoFilePath);
            if (!_fileProvider.FileExists(filePath))
            {
                //file doesn't exist, so try to get only installed plugin names from the obsolete file
                InstalledPluginNames = GetObsoleteInstalledPluginNames();

                //and save info into a new file
                Save();
            }

            //try to get plugin info from the JSON file
            var text = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrEmpty(text))
                return;

            DeserializePluginInfo(text);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the list of all installed plugin names
        /// </summary>
        public virtual IList<string> InstalledPluginNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of plugin names which will be uninstalled
        /// </summary>
        public virtual IList<string> PluginNamesToUninstall { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of plugin names which will be deleted
        /// </summary>
        public virtual IList<string> PluginNamesToDelete { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of plugin names which will be installed
        /// </summary>
        public virtual IList<(string SystemName, Guid? CustomerGuid)> PluginNamesToInstall { get; set; } = new List<(string SystemName, Guid? CustomerGuid)>();

        /// <summary>
        /// Gets or sets the list of plugin names which are not compatible with the current version
        /// </summary>
        [JsonIgnore]
        public virtual IList<string> IncompatiblePlugins { get; set; }

        /// <summary>
        /// Gets or sets the list of assembly loaded collisions
        /// </summary>
        [JsonIgnore]
        public virtual IList<PluginLoadedAssemblyInfo> AssemblyLoadedCollision { get; set; }

        /// <summary>
        /// Gets or sets a collection of plugin descriptors of all deployed plugins
        /// </summary>
        [JsonIgnore]
        public virtual IEnumerable<PluginDescriptor> PluginDescriptors { get; set; }

        #endregion
    }
}