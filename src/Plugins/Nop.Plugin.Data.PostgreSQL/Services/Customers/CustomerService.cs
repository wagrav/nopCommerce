using System;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Events;
using Npgsql;
using NpgsqlTypes;

namespace Nop.Plugin.Data.PostgreSQL.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomerService : Nop.Services.Customers.CustomerService
    {
        #region Fields

        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        public CustomerService(CustomerSettings customerSettings,
            ICacheManager cacheManager,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IRepository<Customer> customerRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<GenericAttribute> gaRepository,
            IStaticCacheManager staticCacheManager) : base (
                customerSettings,
                cacheManager,
                dataProvider,
                dbContext,
                eventPublisher,
                genericAttributeService,
                customerRepository,
                customerCustomerRoleMappingRepository,
                customerPasswordRepository,
                customerRoleRepository,
                gaRepository,
                staticCacheManager)
        {
            this._dbContext = dbContext;
        }

        #endregion

        /// <summary>
        /// Delete guest customer records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete customers only without shopping cart</param>
        /// <returns>Number of deleted customers</returns>
        public override int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart)
        {
            //prepare parameters

            //creating null parameter sql request, NpgsqlParameter vith null value get exception
            var pCreatedFromUtc = "null";

            if (createdFromUtc != null)
            {
                var pgCreatedFromUtc = new NpgsqlParameter("CreatedFromUtc", NpgsqlDbType.Timestamp)
                { Value = createdFromUtc };

                // getting postgresql native datetime string
                pCreatedFromUtc = $"'{pgCreatedFromUtc.NpgsqlValue}'";
            }

            //creating null parameter sql request, NpgsqlParameter vith null value get exception
            var pCreatedToUtc = "null";

            if (createdToUtc != null)
            {
                var pgCreatedToUtc = new NpgsqlParameter("CreatedToUtc", NpgsqlDbType.Timestamp)
                { Value = createdToUtc };

                // getting postgresql native datetime string
                pCreatedToUtc = $"'{pgCreatedToUtc.NpgsqlValue}'";
            }

            //invoke stored procedure
            var resultRow = _dbContext.QueryFromSql<IntQueryType>(
                    $"SELECT * from public.deleteguests({pCreatedFromUtc}, {pCreatedToUtc}, {onlyWithoutShoppingCart})");
            var totalRecordsDeleted = resultRow.ToList()[0].Value ?? 0;

            return totalRecordsDeleted;
        }

    }
}
