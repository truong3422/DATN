using Microsoft.AspNetCore.Authorization;

namespace PoolComVnWebAPI.Authorization
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public int Role { get; set; }

        public CustomAuthorizeAttribute(int role)
        {
            Role = role;
        }
    }
}
