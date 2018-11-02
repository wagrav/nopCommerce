namespace Nop.Plugin.Data.MySQL.Data
{
    /// <summary>
    /// Represents default values related to Nop data
    /// </summary>
    public static partial class NopPostgreSQLDataDefaults
    {
        /// <summary>
        /// Gets a path to the file that contains script to create SQL Server indexes
        /// </summary>
        public static string MySQLIndexesFilePath => "~/Plugins/Data.PostgreSQL/Install/PostgreSQL.Indexes.sql";
    }
}
