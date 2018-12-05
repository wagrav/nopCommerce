using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Nop.Plugin.Data.PostgreSQL.Data
{
    /// <summary>
    /// Represents base object context
    /// </summary>
    public partial class NopObjectContext : Nop.Data.NopObjectContext
    {
        #region Ctor

        public NopObjectContext(DbContextOptions<Nop.Data.NopObjectContext> options) : base(options)
        {
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Modify the input SQL query by adding passed parameters
        /// </summary>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>Modified raw SQL query</returns>
        protected override string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            var paramstring =
                parameters?.Select(p => p as NpgsqlParameter).Where(p => p != null).Select(p => p.ParameterName)
                    .Aggregate(string.Empty, (all, curent) => $"{all}, @{curent}").TrimStart(',', ' ');

            sql = $"SELECT * FROM {sql} ({paramstring ?? string.Empty})";
            return sql;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a LINQ query for the entity based on a raw SQL query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public override IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters)
        {
            return Set<TEntity>().FromSql(CreateSqlWithParameters(sql, parameters), parameters);
        }

        /// <summary>
        /// Drop a table
        /// </summary>
        /// <param name="tableName">Table name</param>
        public override void DropTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            //drop the table
            var dbScript = $"DROP TABLE IF EXISTS \"{tableName}\";";
            ExecuteSqlCommand(dbScript);
            SaveChanges();
        }

        #endregion
    }
}