using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

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
