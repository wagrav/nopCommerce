using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using System;
using System.Linq;

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
                var providerTypes = typeFinder.FindClassesOfType<IDataProvider>();
                var providerType = providerTypes.Select(p => p).Where(p => p.Name == providerName).FirstOrDefault();


                if (providerType != null)
                {

                    // create inctance of current data provider
                    return (IDataProvider)Activator.CreateInstance(providerType);
                }
                else
                {
                    throw new NopException($"Not supported data provider name: '{providerName}'");
                }
            }
        }
        #endregion
    }
}