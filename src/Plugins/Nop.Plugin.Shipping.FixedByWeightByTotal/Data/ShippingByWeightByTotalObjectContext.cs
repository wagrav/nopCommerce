using Microsoft.EntityFrameworkCore;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Shipping.FixedByWeightByTotal.Domain;

namespace Nop.Plugin.Shipping.FixedByWeightByTotal.Data
{
    /// <summary>
    /// Represents plugin object context
    /// </summary>
    public class ShippingByWeightByTotalObjectContext : DbContext
    {
        #region Fields

        private readonly IDbContextOptionsBuilderHelper _opitonsBuilder;

        //represents the ShippingByWeightByTotalRecord entities in the context
        private DbSet<ShippingByWeightByTotalRecord> ShippingByWeightByTotalRecord { get; set; }

        #endregion

        #region Ctor

        public ShippingByWeightByTotalObjectContext(IDbContextOptionsBuilderHelper opitonsBuilder)
        {
            this._opitonsBuilder = opitonsBuilder;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Configure db context options to use database provider.
        /// </summary>
        /// <param name="optionsBuilder">Database context options builder</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var nopConfig = EngineContext.Current.Resolve<NopConfig>();

            var dataSettings = Core.Data.DataSettingsManager.LoadSettings();

            _opitonsBuilder.SetDbContextOptions(optionsBuilder, nopConfig, dataSettings);
        }

        /// <summary>
        /// Generate a script to create all tables for the current model
        /// </summary>
        /// <returns>A SQL script</returns>
        public string GenerateCreateScript()
        {
            return Database.GenerateCreateScript();
        }

        #endregion
    }
}