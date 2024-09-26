using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class TournamentType
    {
        public TournamentType()
        {
            Tournaments = new HashSet<Tournament>();
        }

        public int TournamentTypeId { get; set; }
        public string TournamentTypeName { get; set; } = null!;
        public string? Description { get; set; }
        public int? IsManyStage { get; set; }

        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
