using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class PlayerType
    {
        public PlayerType()
        {
            Tournaments = new HashSet<Tournament>();
        }

        public int PlayerTypeId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
