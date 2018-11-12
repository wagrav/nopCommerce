using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Plugin.Data.PostgreSQL.Services.Catalog
{
    public class ProductService : Nop.Services.Catalog.ProductService
    {
        #region Fileds

        private readonly CatalogSettings _catalogSettings;
        private readonly IDbContext _dbContext;
        private readonly ILanguageService _languageService;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        

        #endregion

        #region Ctor

        public ProductService(CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
            ICacheManager cacheManager,
            IDataProvider dataProvider,
            IDateRangeService dateRangeService,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IRepository<AclRecord> aclRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<Product> productRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<TierPrice> tierPriceRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings)
            : base(catalogSettings,
                commonSettings,
                aclService,
                cacheManager,
                dataProvider,
                dateRangeService,
                dbContext,
                eventPublisher,
                languageService,
                localizationService,
                productAttributeParser,
                productAttributeService,
                aclRepository,
                crossSellProductRepository,
                productRepository,
                productPictureRepository,
                productReviewRepository,
                productWarehouseInventoryRepository,
                relatedProductRepository,
                stockQuantityHistoryRepository,
                storeMappingRepository,
                tierPriceRepository,
                storeMappingService,
                workContext,
                localizationSettings)
        {
            _catalogSettings = catalogSettings;
            _dbContext = dbContext;
            _languageService = languageService;
            _aclRepository = aclRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productRepository = productRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _storeMappingRepository = storeMappingRepository;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        public override IPagedList<Product> SearchProducts(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();
            //products
            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted);
            if (!overridePublished.HasValue)
            {
                //process according to "showHidden"
                if (!showHidden)
                {
                    query = query.Where(p => p.Published);
                }
            }
            else if (overridePublished.Value)
            {
                //published only
                query = query.Where(p => p.Published);
            }
            else if (!overridePublished.Value)
            {
                //unpublished only
                query = query.Where(p => !p.Published);
            }
            if (visibleIndividuallyOnly)
            {
                query = query.Where(p => p.VisibleIndividually);
            }
            //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
            //That's why we pass the date value
            var nowUtc = DateTime.UtcNow;
            if (markedAsNewOnly)
            {
                query = query.Where(p => p.MarkAsNew);
                query = query.Where(p =>
                    (!p.MarkAsNewStartDateTimeUtc.HasValue || p.MarkAsNewStartDateTimeUtc.Value < nowUtc) &&
                    (!p.MarkAsNewEndDateTimeUtc.HasValue || p.MarkAsNewEndDateTimeUtc.Value > nowUtc));
            }
            if (productType.HasValue)
            {
                var productTypeId = (int)productType.Value;
                query = query.Where(p => p.ProductTypeId == productTypeId);
            }

            if (priceMin.HasValue)
            {
                //min price
                query = query.Where(p => p.Price >= priceMin.Value);
            }
            if (priceMax.HasValue)
            {
                //max price
                query = query.Where(p => p.Price <= priceMax.Value);
            }
            if (!showHidden)
            {
                //available dates
                query = query.Where(p =>
                    (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < nowUtc) &&
                    (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > nowUtc));
            }

            //searching by keyword
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                //search by keyword
                var searchLocalizedValue = false;
                if (languageId > 0)
                {
                    if (showHidden)
                    {
                        searchLocalizedValue = true;
                    }
                    else
                    {
                        //ensure that we have at least two published languages
                        var totalPublishedLanguages = _languageService.GetAllLanguages().Count;
                        searchLocalizedValue = totalPublishedLanguages >= 2;
                    }
                }

                query = from p in query
                        join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
                        from lp in p_lp.DefaultIfEmpty()
                        from mapping in p.ProductProductTagMappings.DefaultIfEmpty()
                        where (p.Name.Contains(keywords)) ||
                              (searchDescriptions && p.ShortDescription.Contains(keywords)) ||
                              (searchDescriptions && p.FullDescription.Contains(keywords)) ||
                              //manufacturer part number
                              (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                              //SKU (exact match)
                              (searchSku && p.Sku == keywords) ||
                              //product tags (exact match)
                              (searchProductTags && mapping.ProductTag.Name == keywords) ||
                              //localized values
                              (searchLocalizedValue && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" &&
                               lp.LocaleKey == "Name" && lp.LocaleValue.Contains(keywords)) ||
                              (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId &&
                               lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" &&
                               lp.LocaleValue.Contains(keywords)) ||
                              (searchDescriptions && searchLocalizedValue && lp.LanguageId == languageId &&
                               lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" &&
                               lp.LocaleValue.Contains(keywords))
                        select p;
            }

            if (!showHidden && !_catalogSettings.IgnoreAcl)
            {
                //ACL (access control list)
                //Access control list. Allowed customer roles
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();

                //pass customer role identifiers as comma-delimited string
                query = from p in query
                        join acl in _aclRepository.Table
                            on new { c1 = p.Id, c2 = "Product" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into p_acl
                        from acl in p_acl.DefaultIfEmpty()
                        where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                        select p;
            }

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                //Store mapping
                query = from p in query
                        join sm in _storeMappingRepository.Table
                            on new { c1 = p.Id, c2 = "Product" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
                        from sm in p_sm.DefaultIfEmpty()
                        where !p.LimitedToStores || storeId == sm.StoreId
                        select p;
            }

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                query = from p in query
                        from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
                        where (!featuredProducts.HasValue || featuredProducts.Value == pc.IsFeaturedProduct)
                        select p;
            }

            //manufacturer filtering
            if (manufacturerId > 0)
            {
                query = from p in query
                        from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                        where (!featuredProducts.HasValue || featuredProducts.Value == pm.IsFeaturedProduct)
                        select p;
            }

            //vendor filtering
            if (vendorId > 0)
            {
                query = query.Where(p => p.VendorId == vendorId);
            }

            //warehouse filtering
            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query.Where(p =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (p.ManageInventoryMethodId == manageStockInventoryMethodId &&
                         p.UseMultipleWarehouses &&
                         p.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((p.ManageInventoryMethodId != manageStockInventoryMethodId ||
                          !p.UseMultipleWarehouses) &&
                         p.WarehouseId == warehouseId));
            }

            //related products filtering
            //if (relatedToProductId > 0)
            //{
            //    query = from p in query
            //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
            //            where (relatedToProductId == rp.ProductId1)
            //            select p;
            //}

            //tag filtering
            if (productTagId > 0)
            {
                query = from p in query
                        from pt in p.ProductProductTagMappings.Where(mapping => mapping.ProductTagId == productTagId)
                        select p;
            }

            //get filterable specification attribute option identifier
            if (loadFilterableSpecificationAttributeOptionIds)
            {
                var querySpecs = from p in query
                                 join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                 where psa.AllowFiltering
                                 select psa.SpecificationAttributeOptionId;
                //only distinct attributes
                filterableSpecificationAttributeOptionIds = querySpecs.Distinct().ToList();
            }

            //search by specs
            if (filteredSpecs != null && filteredSpecs.Any())
            {
                var filteredAttributes = _specificationAttributeOptionRepository.Table
                    .Where(sao => filteredSpecs.Contains(sao.Id)).Select(sao => sao.SpecificationAttributeId).Distinct();

                query = query.Where(p => !filteredAttributes.Except
                (
                    _specificationAttributeOptionRepository.Table.Where(
                            sao => p.ProductSpecificationAttributes.Where(
                                    psa => psa.AllowFiltering && filteredSpecs.Contains(psa.SpecificationAttributeOptionId))
                                .Select(psa => psa.SpecificationAttributeOptionId).Contains(sao.Id))
                        .Select(sao => sao.SpecificationAttributeId).Distinct()
                ).Any());
            }

            //only distinct products (group by ID)
            //if we use standard Distinct() method, then all fields will be compared (low performance)
            //it'll not work in SQL Server Compact when searching products by a keyword)
            query = from p in query
                    group p by p.Id
                into pGroup
                    orderby pGroup.Key
                    select pGroup.FirstOrDefault();

            //sort products
            if (orderBy == ProductSortingEnum.Position && categoryIds != null && categoryIds.Any())
            {
                //category position
                var firstCategoryId = categoryIds[0];
                query = query.OrderBy(p =>
                    p.ProductCategories.FirstOrDefault(pc => pc.CategoryId == firstCategoryId).DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position && manufacturerId > 0)
            {
                //manufacturer position
                query =
                    query.OrderBy(p =>
                        p.ProductManufacturers.FirstOrDefault(pm => pm.ManufacturerId == manufacturerId).DisplayOrder);
            }
            else if (orderBy == ProductSortingEnum.Position)
            {
                //otherwise sort by name
                query = query.OrderBy(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.NameAsc)
            {
                //Name: A to Z
                query = query.OrderBy(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.NameDesc)
            {
                //Name: Z to A
                query = query.OrderByDescending(p => p.Name);
            }
            else if (orderBy == ProductSortingEnum.PriceAsc)
            {
                //Price: Low to High
                query = query.OrderBy(p => p.Price);
            }
            else if (orderBy == ProductSortingEnum.PriceDesc)
            {
                //Price: High to Low
                query = query.OrderByDescending(p => p.Price);
            }
            else if (orderBy == ProductSortingEnum.CreatedOn)
            {
                //creation date
                query = query.OrderByDescending(p => p.CreatedOnUtc);
            }
            else
            {
                //actually this code is not reachable
                query = query.OrderBy(p => p.Name);
            }

            var products = new PagedList<Product>(query, pageIndex, pageSize);

            return products;
        }
        
        #endregion
    }
}
