using Autofac;
using Microsoft.EntityFrameworkCore;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using PostgreSQLContext = Nop.Plugin.Data.PostgreSQL.Data;
using NopContext = Nop.Data;



namespace Nop.Plugin.Data.PostgreSQL.Infrastructure
{
    public class ContextRegistrar : IDbContextRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {

            builder.Register(context => new PostgreSQLContext.NopObjectContext(context.Resolve<DbContextOptions<NopContext.NopObjectContext>>()))
                .As<IDbContext>().InstancePerLifetimeScope();

        }

        /// <summary>
        /// After the default dependency registrar.
        /// </summary>
        public int Order => 1;
    }
}
