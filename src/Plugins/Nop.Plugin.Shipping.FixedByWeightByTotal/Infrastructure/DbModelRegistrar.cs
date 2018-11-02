using Microsoft.EntityFrameworkCore;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Shipping.FixedByWeightByTotal.Data;

namespace Nop.Plugin.Shipping.FixedByWeightByTotal.Infrastructure
{
    public class DbModelRegistrar : IDbModelRegistrar
    {
        /// <summary>
        /// Register database model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        public void ModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ShippingByWeightByTotalRecordMap());
        }

        /// <summary>
        /// Order of this database model registrar implementation
        /// </summary>
        public int Order => 1;
    }
}
