using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class SoloMatch
    {
        public SoloMatch()
        {
            PlayerInSoloMatches = new HashSet<PlayerInSoloMatch>();
        }

        public int SoloMatchId { get; set; }
        public DateTime StartTime { get; set; }
        public int GameTypeId { get; set; }
        public int ClubId { get; set; }
        public string Description { get; set; } = null!;
        public byte Status { get; set; }
        public string? Flyer { get; set; }
        public int? RaceTo { get; set; }
        public DateTime? EndTime { get; set; }

        public virtual Club Club { get; set; } = null!;
        public virtual GameType GameType { get; set; } = null!;
        public virtual ICollection<PlayerInSoloMatch> PlayerInSoloMatches { get; set; }
    }
}
