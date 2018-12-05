using Microsoft.EntityFrameworkCore;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Pickup.PickupInStore.Domain;

namespace Nop.Plugin.Pickup.PickupInStore.Data
{
    /// <summary>
    /// Represents plugin object context
    /// </summary>
    public class StorePickupPointObjectContext : DbContext
    {
        #region Fields

        private readonly IDbContextOptionsBuilderHelper _opitonsBuilder;

        private DbSet<StorePickupPoint> StorePickupPoint { get; set; }

        #endregion

        #region Ctor

        public StorePickupPointObjectContext(IDbContextOptionsBuilderHelper opitonsBuilder)
        {
            this._opitonsBuilder = opitonsBuilder;
        }

        #endregion

        #region Methods

        public string GenerateCreateScript()
        {
            return Database.GenerateCreateScript();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var nopConfig = EngineContext.Current.Resolve<NopConfig>();

            var dataSettings = Core.Data.DataSettingsManager.LoadSettings();

            _opitonsBuilder.SetDbContextOptions(optionsBuilder, nopConfig, dataSettings);

        }

        #endregion
    }
}