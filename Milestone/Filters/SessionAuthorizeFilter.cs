using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Milestone.Filters {
    public class SessionAuthorizeFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(m => m is AllowAnonymousAttribute);

            if (allowAnonymous) return;

            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                context.Result = new RedirectToActionResult("Login", "Auth", null);
        }
    }
}
