using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;

namespace Nop.Data
{
    /// <summary>
    /// Represents the Entity Framework data provider manager
    /// </summary>
    public partial class EfDataProviderManager : IDataProviderManager
    {
        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        public IDataProvider DataProvider
        {
            get
            {
                //get current provider type from DataSettings
                var providerName = DataSettingsManager.LoadSettings()?.DataProvider;
                var typeFinder = new WebAppTypeFinder();
                var provider = typeFinder.FindClassesOfType<IDataProvider>()
                    .Select(providerType => (IDataProvider)Activator.CreateInstance(providerType))
                    .FirstOrDefault(p => p.DataProviderName.Equals(providerName, StringComparison.CurrentCultureIgnoreCase));

                if (provider == null)
                    throw new NopException($"Not supported data provider name: '{providerName}'");

                DataBaseManager.DataProvider = provider;

                // create instance of current data provider
                return provider;
            }
        }

        #endregion
    }
}