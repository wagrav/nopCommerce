using Microsoft.EntityFrameworkCore;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Tax.FixedOrByCountryStateZip.Data;

namespace Nop.Plugin.Tax.FixedOrByCountryStateZip.Infrastructure
{
    public class DbModelRegistrar : IDbModelRegistrar
    {
        /// <summary>
        /// Register database model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        public void ModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TaxRateMap());
        }

        /// <summary>
        /// Order of this database model registrar implementation
        /// </summary>
        public int Order => 1;
    }
}