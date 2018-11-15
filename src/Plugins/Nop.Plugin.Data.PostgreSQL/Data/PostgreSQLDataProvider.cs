using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Plugin.Data.PostgreSQL.Data.Extenisions;

namespace Nop.Plugin.Data.PostgreSQL.Data
{
    /// <summary>
    /// Represents SQL Server data provider
    /// </summary>
    public class PostgreSQLDataProvider : IDataProvider
    {
        #region Methods

        /// <summary>
        /// Initialize database
        /// </summary>
        public virtual void InitializeDatabase()
        {
            var context = EngineContext.Current.Resolve<IDbContext>();

            //check some of table names to ensure that we have nopCommerce 2.00+ installed
            var tableNamesToValidate = new List<string> { "Customer", "Discount", "Order", "Product", "ShoppingCartItem" };
            var existingTableNames = context
                .QueryFromSql<StringQueryType>(
                    $"SELECT table_name AS \"Value\" FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE' and table_catalog = '{context.DbName()}'")
                .Select(stringValue => stringValue.Value).ToList();
            var createTables = !existingTableNames.Intersect(tableNamesToValidate, StringComparer.InvariantCultureIgnoreCase).Any();
            if (!createTables)
                return;

            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

            //create tables
            context.ExecuteSqlScript(context.GenerateCreateScript());

            //create indexes
            context.ExecutePostgreSqlScriptFromFile(fileProvider.MapPath(NopPostgreSQLDataDefaults.PostgreSqlIndexesFilePath));

            //create stored procedures 
            context.ExecutePostgreSqlScriptFromFile(fileProvider.MapPath(NopPostgreSQLDataDefaults.PostgreSqlStoredProceduresFilePath));
        }

        /// <summary>
        /// Get a support database parameter object (used by stored procedures)
        /// </summary>
        /// <returns>Parameter</returns>
        public virtual DbParameter GetParameter()
        {
            return new SqlParameter();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        public virtual bool BackupSupported => true;

        /// <summary>
        /// Gets a maximum length of the data for HASHBYTES functions, returns 0 if HASHBYTES function is not supported
        /// </summary>
        public virtual int SupportedLengthOfBinaryHash => 0;

        /// <summary>
        /// Gets a data provider name
        /// </summary>
        public string DataProviderName => "PostgreSQL";

        #endregion
    }
}
