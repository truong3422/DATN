using Microsoft.AspNetCore.Authorization;

namespace PoolComVnWebAPI.Authorization
{
    public class CustomAuthorizationRequirement : IAuthorizationRequirement
    {
        public int RequiredRoleId { get; }

        public CustomAuthorizationRequirement(int requiredRoleId)
        {
            RequiredRoleId = requiredRoleId;
        }
    }
}
