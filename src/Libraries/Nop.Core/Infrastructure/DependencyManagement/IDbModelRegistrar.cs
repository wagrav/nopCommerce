using Microsoft.EntityFrameworkCore;

namespace Nop.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// Database model registrar interface
    /// </summary>
    public interface IDbModelRegistrar
    {
        /// <summary>
        /// Register batabase model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        void ModelCreating(ModelBuilder modelBuilder);

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        int Order { get; }
    }
}
