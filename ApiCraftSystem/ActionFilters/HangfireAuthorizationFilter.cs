using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace ApiCraftSystem.ActionFilters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Example: Require authenticated users
            return httpContext.User.Identity?.IsAuthenticated == true;

            // Example with role check:
            // return httpContext.User.Identity?.IsAuthenticated == true &&
            //        httpContext.User.IsInRole("Admin");
        }
    }
}
