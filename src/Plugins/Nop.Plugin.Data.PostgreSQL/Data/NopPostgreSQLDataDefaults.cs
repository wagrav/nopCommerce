namespace Nop.Plugin.Data.PostgreSQL.Data
{
    /// <summary>
    /// Represents default values related to Nop data
    /// </summary>
    public static partial class NopPostgreSQLDataDefaults
    {
        /// <summary>
        /// Gets a path to the file that contains script to create PostgreSQL Server indexes
        /// </summary>
        public static string PostgreSqlIndexesFilePath => "~/Plugins/Data.PostgreSQL/Install/PostgeSQL.Indexes.sql";

        /// <summary>
        /// Gets a path to the file that contains script to create PostgreSQL Server stored procedures
        /// </summary>
        public static string PostgreSqlStoredProceduresFilePath => "~/Plugins/Data.PostgreSQL/Install/PostgeSQL.StoredProcedures.sql";

        /// <summary>
        /// Gets a data provider name
        /// </summary>
        public static string DataProviderName => "PostgreSQL";
    }
}
