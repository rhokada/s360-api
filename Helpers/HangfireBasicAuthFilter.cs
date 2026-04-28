using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;

namespace WebApi.Helpers
{
    public class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
    {
        private readonly string _user;
        private readonly string _pass;

        public HangfireBasicAuthFilter(string user, string pass)
        {
            _user = user;
            _pass = pass;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var header = httpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic "))
            {
                Challenge(httpContext);
                return false;
            }

            try
            {
                var encoded = header.Substring("Basic ".Length).Trim();
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = decoded.Split(':', 2);
                if (parts.Length == 2 && parts[0] == _user && parts[1] == _pass)
                    return true;
            }
            catch { }

            Challenge(httpContext);
            return false;
        }

        private static void Challenge(HttpContext ctx)
        {
            ctx.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire\"";
            ctx.Response.StatusCode = 401;
        }
    }
}
