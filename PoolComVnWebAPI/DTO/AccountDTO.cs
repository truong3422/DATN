using BusinessObject.Models;
using System.ComponentModel.DataAnnotations;

namespace PoolComVnWebAPI.DTO
{
    public class AccountDTO
    {
        public int AccountID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public string? PhoneNumber { get; set; }
        public string? verifyCode { get; set; }
        public bool Status { get; set; }
    }
}
