using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Data;

namespace Nop.Plugin.Data.PostgreSQL.Data
{
    /// <summary>
    /// MySQL db context options helper to use MySQL for db context.
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
            var dbContextOptionsBuilder = optionsBuilder.UseLazyLoadingProxies();

            if (!dataSettings?.IsValid ?? true)
                return;

            if (!dataSettings?.IsValid ?? true)
                return;
            dbContextOptionsBuilder.UseNpgsql(dataSettings.DataConnectionString);
        }
    }
}
