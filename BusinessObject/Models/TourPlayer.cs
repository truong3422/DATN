using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class TourPlayer
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int TournamentId { get; set; }

        public virtual Player Player { get; set; } = null!;
        public virtual Tournament Tournament { get; set; } = null!;
    }
}
