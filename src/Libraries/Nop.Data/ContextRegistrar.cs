using Autofac;
using Microsoft.EntityFrameworkCore;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;

namespace Nop.Data
{
    public class ContextRegistrar : IDbContextRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {

            builder.Register(context => new NopObjectContext(context.Resolve<DbContextOptions<NopObjectContext>>()))
                        .As<IDbContext>().InstancePerLifetimeScope();

        }

        /// <summary>
        /// After the default dependency registrar.
        /// </summary>
        public int Order => 1;
    }
}
