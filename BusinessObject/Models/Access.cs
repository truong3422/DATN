using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Access
    {
        public Access()
        {
            Tournaments = new HashSet<Tournament>();
        }

        public int AccessId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
