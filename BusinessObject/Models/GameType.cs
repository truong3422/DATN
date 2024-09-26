using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class GameType
    {
        public GameType()
        {
            SoloMatches = new HashSet<SoloMatch>();
            Tournaments = new HashSet<Tournament>();
        }

        public int GameTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<SoloMatch> SoloMatches { get; set; }
        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
