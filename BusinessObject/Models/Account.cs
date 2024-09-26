using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Account
    {
        public Account()
        {
            News = new HashSet<News>();
            Users = new HashSet<User>();
        }

        public int AccountId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VerifyCode { get; set; }
        public bool Status { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual Club? Club { get; set; }
        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
