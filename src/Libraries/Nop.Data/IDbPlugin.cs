using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nop.Data
{
    public partial interface IDbPlugin
    {
        /// <summary>
        /// Check install
        /// </summary>
        /// <param name="model">Installation model</param>
        /// <param name="modelState"> State of model</param>
        void CheckModel(IDbPluginInstallModel model, ModelStateDictionary modelState);

        /// <summary>
        /// Creates a database on the PosgreSQL server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="collation">Server collation; the default one will be used if not specified</param>
        /// <param name="triesToConnect">
        /// Number of times to try to connect to database. 
        /// If connection cannot be open, then error will be returned. 
        /// Pass 0 to skip this validation.
        /// </param>
        /// <returns>Error</returns>
        string CreateDatabase(string connectionString, string collation, int triesToConnect = 10);

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        bool DatabaseExists(string connectionString);

        /// <summary>
        /// Gets a DbProvider view URL
        /// </summary>
        string DbProvider();

        /// <summary>
        /// Gets a DbConnectionInfo view URL
        /// </summary>
        string DbConnectionInfo();


        /// <summary>
        /// Create connection strings
        /// </summary>
        /// <param name="model">Installation model</param>
        /// <returns>Connection string</returns>
        string GetConnectionString(IDbPluginInstallModel model);

        /// <summary>
        /// Returns provider name
        /// </summary>
        string DataProviderName { get; }
    }
}
