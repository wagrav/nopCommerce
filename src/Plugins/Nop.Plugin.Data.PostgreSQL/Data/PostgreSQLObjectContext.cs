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
        public NopObjectContext(DbContextOptions<Nop.Data.NopObjectContext> options) : base(options)
        {
        }

        public override IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters)
        {
            return Set<TEntity>().FromSql(CreateSqlWithParameters(sql, parameters), parameters);
        }

        protected override string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            var paramstring =
                parameters?.Select(p => p as NpgsqlParameter).Where(p => p != null).Select(p => p.ParameterName)
                    .Aggregate(string.Empty, (all, curent) => $"{all}, @{curent}").TrimStart(',', ' ');

            sql = $"SELECT * FROM {sql} ({paramstring ?? string.Empty})";
            return sql;
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
    }
}