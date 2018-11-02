using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Data;

namespace Nop.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Represents extensions of DbContextOptionsBuilder
    /// </summary>
    public static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// SQL Server specific extension method for Microsoft.EntityFrameworkCore.DbContextOptionsBuilder
        /// </summary>
        /// <param name="optionsBuilder">Database context options builder</param>
        /// <param name="services">Collection of service descriptors</param>
        public static void UseSqlServerWithLazyLoading(this DbContextOptionsBuilder optionsBuilder, IServiceCollection services)
        {
            var nopConfig = services.BuildServiceProvider().GetRequiredService<NopConfig>();

            var dataProviderType = DataSettingsManager.DataProviderType;

            var dataSettings = DataSettingsManager.LoadSettings();
            if (!dataSettings?.IsValid ?? true)
                return;

            var dbContext = EngineContext.Current.Resolve<IDbContextOptionsBuilderHelper>();
            dbContext.Configure(optionsBuilder, services, nopConfig, dataSettings);
        }
    }
}
