using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Catalog;
using Nop.Services.Common;
//using ProductService = Nop.Plugin.Data.PostgreSQL.Services.Catalog.ProductService;
using ProductTagService = Nop.Plugin.Data.PostgreSQL.Services.Catalog.ProductTagService;
using FulltextService = Nop.Plugin.Data.PostgreSQL.Services.Common.FulltextService;
//using LocalizationService = Nop.Plugin.Data.PostgreSQL.Services.Localization.LocalizationService;
using CategoryService = Nop.Plugin.Data.PostgreSQL.Services.Catalog.CategoryService;

namespace Nop.Plugin.Data.PostgreSQL.Infrastructure
{
    public class DependencyRegistrar : IDbDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //services
            //builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductTagService>().As<IProductTagService>().InstancePerLifetimeScope();
            builder.RegisterType<FulltextService>().As<IFulltextService>().InstancePerLifetimeScope();
            //builder.RegisterType<LocalizationService>().As<ILocalizationService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// After the default dependency registrar.
        /// </summary>
        public int Order => 1;
    }
}
