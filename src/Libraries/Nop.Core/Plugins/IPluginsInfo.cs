using System;
using System.Collections.Generic;

namespace Nop.Core.Plugins
{
    /// <summary>
    /// Represents an information about plugins
    /// </summary>
    public interface IPluginsInfo
    {
        /// <summary>
        /// Save plugins info to the file
        /// </summary>
        void Save();

        /// <summary>
        /// Get plugins info
        /// </summary>
        void LoadPluginInfo();

        /// <summary>
        /// Gets or sets the list of all installed plugin names
        /// </summary>
        IList<string> InstalledPluginNames { get; set; }

        /// <summary>
        /// Gets or sets the list of plugin names which will be uninstalled
        /// </summary>
        IList<string> PluginNamesToUninstall { get; set; }

        /// <summary>
        /// Gets or sets the list of plugin names which will be deleted
        /// </summary>
        IList<string> PluginNamesToDelete { get; set; }

        /// <summary>
        /// Gets or sets the list of plugin names which will be installed
        /// </summary>
        IList<(string SystemName, Guid? CustomerGuid)> PluginNamesToInstall { get; set; }

        /// <summary>
        /// Gets or sets the list of assembly loaded collisions
        /// </summary>
        IList<PluginLoadedAssemblyInfo> AssemblyLoadedCollision { get; set; }

        /// <summary>
        /// Gets or sets a collection of plugin descriptors of all deployed plugins
        /// </summary>
        IEnumerable<PluginDescriptor> PluginDescriptors { get; set; }

        /// <summary>
        /// Gets or sets the list of plugin names which are not compatible with the current version
        /// </summary>
        IList<string> IncompatiblePlugins { get; set; }
    }
}