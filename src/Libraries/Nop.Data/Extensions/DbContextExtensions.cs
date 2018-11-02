using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Nop.Core;

namespace Nop.Data.Extensions
{
    /// <summary>
    /// Represents database context extensions
    /// </summary>
    public static class DbContextExtensions
    {
        #region Fields
        
        private static readonly ConcurrentDictionary<string, string> _tableNames = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, IEnumerable<(string, int?)>> _columnsMaxLength = new ConcurrentDictionary<string, IEnumerable<(string, int?)>>();
        private static readonly ConcurrentDictionary<string, IEnumerable<(string, decimal?)>> _decimalColumnsMaxValue = new ConcurrentDictionary<string, IEnumerable<(string, decimal?)>>();
        private static string _databaseName;

        #endregion

        #region Utilities

        /// <summary>
        /// Loads a copy of the entity using the passed function
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <param name="entity">Entity</param>
        /// <param name="getValuesFunction">Function to get the values of the tracked entity</param>
        /// <returns>Copy of the passed entity</returns>
        private static TEntity LoadEntityCopy<TEntity>(IDbContext context, TEntity entity, Func<EntityEntry<TEntity>, PropertyValues> getValuesFunction)
            where TEntity : BaseEntity
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");

            //try to get the entity tracking object
            var entityEntry = dbContext.ChangeTracker.Entries<TEntity>().FirstOrDefault(entry => entry.Entity == entity);
            if (entityEntry == null)
                return null;

            //get a copy of the entity
            var entityCopy = getValuesFunction(entityEntry)?.ToObject() as TEntity;

            return entityCopy;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the original copy of the entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <param name="entity">Entity</param>
        /// <returns>Copy of the passed entity</returns>
        public static TEntity LoadOriginalCopy<TEntity>(this IDbContext context, TEntity entity) where TEntity : BaseEntity
        {
            return LoadEntityCopy(context, entity, entityEntry => entityEntry.OriginalValues);
        }

        /// <summary>
        /// Loads the database copy of the entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <param name="entity">Entity</param>
        /// <returns>Copy of the passed entity</returns>
        public static TEntity LoadDatabaseCopy<TEntity>(this IDbContext context, TEntity entity) where TEntity : BaseEntity
        {
            return LoadEntityCopy(context, entity, entityEntry => entityEntry.GetDatabaseValues());
        }

        /// <summary>
        /// Drop a plugin table
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="tableName">Table name</param>
        public static void DropPluginTable(this IDbContext context, string tableName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            //drop the table
            var dbScript = $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE [{tableName}]";
            context.ExecuteSqlCommand(dbScript);
            context.SaveChanges();
        }

        /// <summary>
        /// Get table name of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <returns>Table name</returns>
        public static string GetTableName<TEntity>(this IDbContext context) where TEntity : BaseEntity
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");
            
            var entityTypeFullName = typeof(TEntity).FullName;
            if (!_tableNames.ContainsKey(entityTypeFullName))
            {
                //get entity type
                var entityType = dbContext.Model.FindRuntimeEntityType(typeof(TEntity));

                //get the name of the table to which the entity type is mapped
                _tableNames.TryAdd(entityTypeFullName, entityType.Relational().TableName);
            }

            _tableNames.TryGetValue(entityTypeFullName, out var tableName);

            return tableName;
        }

        /// <summary>
        /// Gets the maximum lengths of data that is allowed for the entity properties
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <returns>Collection of name - max length pairs</returns>
        public static IEnumerable<(string Name, int? MaxLength)> GetColumnsMaxLength<TEntity>(this IDbContext context) where TEntity : BaseEntity
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");

            var entityTypeFullName = typeof(TEntity).FullName;
            if (!_columnsMaxLength.ContainsKey(entityTypeFullName))
            {
                //get entity type
                var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

                //get property name - max length pairs
                _columnsMaxLength.TryAdd(entityTypeFullName, 
                    entityType.GetProperties().Select(property => (property.Name, property.GetMaxLength())));
            }

            _columnsMaxLength.TryGetValue(entityTypeFullName, out var result);

            return result;
        }

        /// <summary>
        /// Get maximum decimal values
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <returns>Collection of name - max decimal value pairs</returns>
        public static IEnumerable<(string Name, decimal? MaxValue)> GetDecimalColumnsMaxValue<TEntity>(this IDbContext context)
            where TEntity : BaseEntity
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");

            var entityTypeFullName = typeof(TEntity).FullName;
            if (!_decimalColumnsMaxValue.ContainsKey(entityTypeFullName))
            {
                //get entity type
                var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

                //get entity decimal properties
                var properties = entityType.GetProperties().Where(property => property.ClrType == typeof(decimal));

                //return property name - max decimal value pairs
                _decimalColumnsMaxValue.TryAdd(entityTypeFullName, properties.Select(property =>
                {
                    var mapping = new RelationalTypeMappingInfo(property);
                    if (!mapping.Precision.HasValue || !mapping.Scale.HasValue)
                        return (property.Name, null);

                    return (property.Name, new decimal?((decimal)Math.Pow(10, mapping.Precision.Value - mapping.Scale.Value)));
                }));
            }

            _decimalColumnsMaxValue.TryGetValue(entityTypeFullName, out var result);

            return result;
        }

        /// <summary>
        /// Get database name
        /// </summary>
        /// <param name="context">Database context</param>
        /// <returns>Database name</returns>
        public static string DbName(this IDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");

            if (!string.IsNullOrEmpty(_databaseName)) 
                return _databaseName;

            //get database connection
            var dbConnection = dbContext.Database.GetDbConnection();

            //return the database name
            _databaseName = dbConnection.Database;

            return _databaseName;
        }

        #endregion
    }
}