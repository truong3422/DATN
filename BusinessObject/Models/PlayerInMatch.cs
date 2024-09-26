using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class PlayerInMatch
    {
        public int PlayerMatchId { get; set; }
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public int? Score { get; set; }
        public string? GameWin { get; set; }
        public bool? IsWinner { get; set; }

        public virtual MatchOfTournament Match { get; set; } = null!;
        public virtual Player Player { get; set; } = null!;
    }
}
