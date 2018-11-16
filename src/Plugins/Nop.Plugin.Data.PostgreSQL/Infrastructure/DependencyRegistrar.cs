using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Data.PostgreSQL.Infrastructure
{
    public class DependencyRegistrar : IDbDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //services
            builder.RegisterType<Services.Catalog.ProductService>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<Services.Catalog.ProductTagService>().As<IProductTagService>().InstancePerLifetimeScope();
            builder.RegisterType<Services.Common.FulltextService>().As<IFulltextService>().InstancePerLifetimeScope();
            builder.RegisterType<Services.Catalog.CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
            builder.RegisterType<Services.Customers.CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// After the default dependency registrar.
        /// </summary>
        public int Order => 1;
    }
}
