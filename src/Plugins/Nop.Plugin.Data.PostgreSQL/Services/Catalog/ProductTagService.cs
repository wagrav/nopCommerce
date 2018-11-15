using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Seo;

namespace Nop.Plugin.Data.PostgreSQL.Services.Catalog
{
    public class ProductTagService : Nop.Services.Catalog.ProductTagService
    {
        #region Fileds

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IDbContext _dbContext;

        #endregion

        #region Ctor

        public ProductTagService(ICacheManager cacheManager,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            IProductService productService,
            IRepository<ProductProductTagMapping> productProductTagMappingRepository,
            IRepository<ProductTag> productTagRepository,
            IStaticCacheManager staticCacheManager,
            IUrlRecordService urlRecordService)
            : base(cacheManager,
                dbContext,
                eventPublisher,
                productService,
                productProductTagMappingRepository,
                productTagRepository,
                staticCacheManager,
                urlRecordService)
        {
            _staticCacheManager = staticCacheManager;
            _dbContext = dbContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get product count for each of existing product tag
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Dictionary of "product tag ID : product count"</returns>
        protected override Dictionary<int, int> GetProductCount(int storeId)
        {
            var key = string.Format(NopCatalogDefaults.ProductTagCountCacheKey, storeId);
            return _staticCacheManager.Get(key, () =>
            {
                return _dbContext.QueryFromSql<ProductTagWithCount>($"SELECT * From producttagcountloadall({storeId})")
                    .ToDictionary(item => item.ProductTagId, item => item.ProductCount);
            });
        }

        #endregion
    }
}
