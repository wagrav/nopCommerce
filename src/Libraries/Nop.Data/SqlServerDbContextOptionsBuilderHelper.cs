using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Data;

namespace Nop.Data
{
    /// <summary>
    /// MySQL db context options helper to use MySQL for db context.
    /// </summary>
    public class SqlServerDbContextOptionsBuilderHelper : IDbContextOptionsBuilderHelper
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
            if (!dataSettings?.IsValid ?? true)
                return;

            //register copitns for Ms SqlServer
            var dbContextOptionsBuilder = optionsBuilder.UseLazyLoadingProxies();

            if (nopConfig.UseRowNumberForPaging)
                dbContextOptionsBuilder.UseSqlServer(dataSettings.DataConnectionString, option => option.UseRowNumberForPaging());
            else
                dbContextOptionsBuilder.UseSqlServer(dataSettings.DataConnectionString);
        }
    }
}
