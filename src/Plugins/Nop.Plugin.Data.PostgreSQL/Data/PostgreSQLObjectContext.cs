using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Data;
using Nop.Data.Mapping;

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

        public override IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters)// where TEntity : BaseEntity
        {
            return this.Set<TEntity>().FromSql(CreateSqlWithParameters(sql, parameters), parameters);
        }

        protected override string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            var paramstring = String.Empty;
            for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
            {
                if (!(parameters[i] is Npgsql.NpgsqlParameter parameter))
                    continue;
                paramstring = $"{paramstring}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";
            }
            paramstring = paramstring.TrimEnd(',');
            sql = $"SELECT * FROM {sql} ({paramstring})";

            return sql;
        }

    }



}