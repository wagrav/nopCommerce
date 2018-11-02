using Autofac;
using Microsoft.EntityFrameworkCore;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using PostgreSQLContext = Nop.Plugin.Data.PostgreSQL.Data;

namespace Nop.Plugin.Data.PostgreSQL.Infrastructure
{
    public class ContextRegistrar : IDbContextRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.Register(context => new PostgreSQLContext.NopObjectContext(context.Resolve<DbContextOptions<NopObjectContext>>()))
                .As<IDbContext>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// After the default dependency registrar.
        /// </summary>
        public int Order => 1;
    }
}
