using Autofac;
using Nop.Core.Configuration;

namespace Nop.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// Database dependency registrar interface
    /// </summary>
    public interface IDbDependencyRegistrar
    {
        /// <summary>
        /// Register  services and interfaces specific for DB
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config);

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        int Order { get; }
    }
}
