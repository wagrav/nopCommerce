using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Data;

namespace Nop.Plugin.Data.PostgreSQL.Data
{
    /// <summary>
    /// PostgreSQL db context options helper to use PostgreSQL for db context.
    /// </summary>
    public class PostgreSQLDbContextOptionsBuilderHelper : IDbContextOptionsBuilderHelper
    {
        /// <summary>
        /// Configure db context options to use PostgreSQL.
        /// </summary>
        /// <param name="optionsBuilder">DbContextOptionsBuilder</param>
        /// <param name="services">IServiceCollection</param>
        /// <param name="nopConfig">NopConfig</param>
        /// <param name="dataSettings">DataSettings</param>
        public void Configure(DbContextOptionsBuilder optionsBuilder, IServiceCollection services, NopConfig nopConfig, DataSettings dataSettings)
        {
            SetDbContextOptions(optionsBuilder, nopConfig, dataSettings);
        }

        /// <summary>
        /// Configure db context options to use PostgreSQL.
        /// </summary>
        /// <param name="optionsBuilder">DbContextOptionsBuilder</param>
        /// <param name="nopConfig">NopConfig</param>
        /// <param name="dataSettings">DataSettings</param>
        public void SetDbContextOptions(DbContextOptionsBuilder optionsBuilder, NopConfig nopConfig, DataSettings dataSettings)
        {
            var dbContextOptionsBuilder = optionsBuilder.UseLazyLoadingProxies();

            if (!dataSettings?.IsValid ?? true)
                return;

            dbContextOptionsBuilder.UseNpgsql(dataSettings.DataConnectionString);
        }
    }
}
