using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PoolComVnWebAPI.Authorization
{
    public class CustomAuthorizationHandler : AuthorizationHandler<CustomAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomAuthorizationRequirement requirement)
        {
            // Kiểm tra xem có tồn tại claim "roleId" không
            var roleClaim = context.User.FindFirst("roleId");

            if (roleClaim != null && int.TryParse(roleClaim.Value, out var userRoleId))
            {
                // Kiểm tra xem role của người dùng có khớp với yêu cầu không
                if (userRoleId == requirement.RequiredRoleId)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
