using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class PlayerInSoloMatch
    {
        public int PlayerSoloMatchId { get; set; }
        public int SoloMatchId { get; set; }
        public int PlayerId { get; set; }
        public int? Score { get; set; }
        public string? GameWin { get; set; }
        public bool? IsWinner { get; set; }

        public virtual Player Player { get; set; } = null!;
        public virtual SoloMatch SoloMatch { get; set; } = null!;
    }
}
