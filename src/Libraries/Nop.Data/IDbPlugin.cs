using Microsoft.AspNetCore.Mvc;
using Nop.Core.Plugins;

namespace Nop.Data
{
    public partial interface IDbPlugin
    {
        string DbProvider();

        string DbConnectionInfo();
    }
}
