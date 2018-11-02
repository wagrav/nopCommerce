using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Data.PostgreSQL.Services.Catalog
{
    public class CategoryService : Nop.Services.Catalog.CategoryService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IDbContext _dbContext;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly string _entityName;

        #endregion

        #region Ctor

        public CategoryService(CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
            ICacheManager cacheManager,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IRepository<AclRecord> aclRepository,
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IWorkContext workContext) : base(catalogSettings,
            commonSettings,
            aclService,
            cacheManager,
            dataProvider,
            dbContext,
            eventPublisher,
            localizationService,
            aclRepository,
            categoryRepository,
            productRepository,
            productCategoryRepository,
            storeMappingRepository,
            staticCacheManager,
            storeContext,
            storeMappingService,
            workContext)
        {
            this._catalogSettings = catalogSettings;
            this._commonSettings = commonSettings;
            this._aclRepository = aclRepository;
            this._dbContext = dbContext;
            this._categoryRepository = categoryRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._workContext = workContext;
            this._entityName = typeof(Category).Name;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Categories</returns>
        public override IPagedList<Category> GetAllCategories(string categoryName, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (_commonSettings.UseStoredProcedureForLoadingCategories)
            {
                //stored procedures are enabled for loading categories and supported by the database. 
                //It's much faster with a large number of categories than the LINQ implementation below 

                //prepare parameters
                //var showHiddenParameter = _dataProvider.GetBooleanParameter("ShowHidden", showHidden);
                var showHiddenParameter = new Npgsql.NpgsqlParameter("showhidden", NpgsqlTypes.NpgsqlDbType.Boolean)
                    { Value = showHidden };

                //var nameParameter = _dataProvider.GetStringParameter("Name", categoryName ?? string.Empty);
                var nameParameter = new Npgsql.NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text)
                    { Value = categoryName ?? string.Empty };

                //var storeIdParameter = _dataProvider.GetInt32Parameter("StoreId", !_catalogSettings.IgnoreStoreLimitations ? storeId : 0);
                var storeIdParameter = new Npgsql.NpgsqlParameter("storeid", NpgsqlTypes.NpgsqlDbType.Integer)
                    { Value = !_catalogSettings.IgnoreStoreLimitations ? storeId : 0 };

                //var pageIndexParameter = _dataProvider.GetInt32Parameter("PageIndex", pageIndex);
                var pageIndexParameter = new Npgsql.NpgsqlParameter("pageindex", NpgsqlTypes.NpgsqlDbType.Integer)
                { Value = pageIndex };

                //var pageSizeParameter = _dataProvider.GetInt32Parameter("PageSize", pageSize);
                var pageSizeParameter = new Npgsql.NpgsqlParameter("pagesize", NpgsqlTypes.NpgsqlDbType.Integer)
                { Value = pageSize };

                //pass allowed customer role identifiers as comma-delimited string
                var customerRoleIds = !_catalogSettings.IgnoreAcl ? string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()) : string.Empty;
                var customerRoleIdsParameter = new Npgsql.NpgsqlParameter("customerroleids", NpgsqlTypes.NpgsqlDbType.Text)
                { Value = customerRoleIds };

                //invoke categoryloadallpaged stored procedure
                var categories = _dbContext.EntityFromSql<Category>("categoryloadallpaged",
                    showHiddenParameter, nameParameter, storeIdParameter, customerRoleIdsParameter,
                    pageIndexParameter, pageSizeParameter).ToList();

                //invoke categoryloadallcount stored procedure
                //always returns table with single row and single collumn named "Value"
                var resultRow = _dbContext.QueryFromSql<IntQueryType>(
                    $"SELECT * from public.categoryloadallcount({showHidden}, '{categoryName}', {storeId}, '{customerRoleIds}')");
                var totalRecords = resultRow.ToList()[0].Value ?? 0;
                //paging
                return new PagedList<Category>(categories, pageIndex, pageSize, totalRecords);
            }

            //don't use a stored procedure. Use LINQ
            var query = _categoryRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(c => c.Name.Contains(categoryName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id);

            if ((storeId > 0 && !_catalogSettings.IgnoreStoreLimitations) || (!showHidden && !_catalogSettings.IgnoreAcl))
            {
                if (!showHidden && !_catalogSettings.IgnoreAcl)
                {
                    //ACL (access control list)
                    var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                    query = from c in query
                            join acl in _aclRepository.Table
                                on new { c1 = c.Id, c2 = _entityName } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                            from acl in c_acl.DefaultIfEmpty()
                            where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                            select c;
                }

                if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
                {
                    //Store mapping
                    query = from c in query
                            join sm in _storeMappingRepository.Table
                                on new { c1 = c.Id, c2 = _entityName } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                            from sm in c_sm.DefaultIfEmpty()
                            where !c.LimitedToStores || storeId == sm.StoreId
                            select c;
                }

                query = query.Distinct().OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            }

            var unsortedCategories = query.ToList();

            //sort categories
            var sortedCategories = SortCategoriesForTree(unsortedCategories);

            //paging
            return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
        }

        #endregion
    }
}
