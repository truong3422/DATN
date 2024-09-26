using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class DrawType
    {
        public DrawType()
        {
            Tournaments = new HashSet<Tournament>();
        }

        public int DrawTypeId { get; set; }
        public string DrawType1 { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
