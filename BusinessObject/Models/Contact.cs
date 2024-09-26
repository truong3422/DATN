using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Contact
    {
        public int ContactId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
