using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Data.PostgreSQL.Data;
using Nop.Web.Controllers;
using System.Linq;

namespace Nop.Plugin.Data.PostgreSQL.Infrastructure
{
    public class Startup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkNpgsql();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// After the NopDbStartup startup.
        /// </summary>
        public int Order => 11;
    }
}
