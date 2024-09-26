using BusinessObject.Models;

namespace PoolComVnWebAPI.Authentication
{
    public class TokenClaim
    {
        public string Email { get; set; }
        public Role Role { get; set; }
    }
}
