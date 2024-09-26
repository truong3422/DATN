using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Tournament
    {
        public Tournament()
        {
            MatchOfTournaments = new HashSet<MatchOfTournament>();
            Players = new HashSet<Player>();
        }

        public int TourId { get; set; }
        public string TourName { get; set; } = null!;
        public string? Description { get; set; }
        public int GameTypeId { get; set; }
        public int TournamentTypeId { get; set; }
        public int PlayerNumber { get; set; }
        public int EntryFee { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RaceToString { get; set; } = null!;
        public DateTime RegistrationDeadline { get; set; }
        public int MaxPlayerNumber { get; set; }
        public byte Status { get; set; }
        public int ClubId { get; set; }
        public bool? Access { get; set; }
        public string? Flyer { get; set; }
        public int? PlayerTypeId { get; set; }
        public int? TotalPrize { get; set; }
        public int? KnockoutPlayerNumber { get; set; }

        public virtual Club Club { get; set; } = null!;
        public virtual GameType GameType { get; set; } = null!;
        public virtual PlayerType? PlayerType { get; set; }
        public virtual TournamentType TournamentType { get; set; } = null!;
        public virtual ICollection<MatchOfTournament> MatchOfTournaments { get; set; }
        public virtual ICollection<Player> Players { get; set; }
    }
}
