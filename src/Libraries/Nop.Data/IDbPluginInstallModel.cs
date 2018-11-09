using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Nop.Data
{
    public partial interface IDbPluginInstallModel
    {
        string DatabaseConnectionString { get; set; }
        string SqlConnectionInfo { get; set; }

        string SqlServerName { get; set; }
        int SqlServerPort { get; set; }
        string SqlDatabaseName { get; set; }
        string SqlServerUsername { get; set; }
        [DataType(DataType.Password)]
        string SqlServerPassword { get; set; }
        string SqlAuthenticationType { get; set; }
        bool SqlServerCreateDatabase { get; set; }

        bool UseCustomCollation { get; set; }
        string Collation { get; set; }
    }
}
