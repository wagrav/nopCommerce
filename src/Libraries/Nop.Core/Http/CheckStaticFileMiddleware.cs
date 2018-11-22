using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Data;

namespace Nop.Core.Http
{
    /// <summary>
    /// Represents middleware that checks is requests static file
    /// </summary>
    public class CheckStaticFileMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        public CheckStaticFileMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke middleware actions
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="webHelper">Web helper</param>
        /// <returns>Task</returns>
        public async Task Invoke(HttpContext context, IWebHelper webHelper)
        {
            if (webHelper.IsStaticResource())
            {
                //change response code to Not Found
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return;
            }

            //or call the next middleware in the request pipeline
            await _next(context);
        }

        #endregion
    }
}