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
        private DbSet<ShippingByWeightByTotalRecord> ShippingByWeightByTotalRecord { get; set; }

        #endregion

        #region Ctor

        public ShippingByWeightByTotalObjectContext(IDbContextOptionsBuilderHelper opitonsBuilder)
        {
            this._opitonsBuilder = opitonsBuilder;
        }
        #endregion

        #region Methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var nopConfig = EngineContext.Current.Resolve<NopConfig>();

            var dataSettings = Core.Data.DataSettingsManager.LoadSettings();

            _opitonsBuilder.SetDbContextOptions(optionsBuilder, nopConfig, dataSettings);
        }

        public string GenerateCreateScript()
        {
            return Database.GenerateCreateScript();
        }

        #endregion
    }
}