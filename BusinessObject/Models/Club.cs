using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Club
    {
        public Club()
        {
            ClubPosts = new HashSet<ClubPost>();
            SoloMatches = new HashSet<SoloMatch>();
            Tables = new HashSet<Table>();
            Tournaments = new HashSet<Tournament>();
        }

        public int ClubId { get; set; }
        public string ClubName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Facebook { get; set; } = null!;
        public string? Avatar { get; set; }
        public int? AccountId { get; set; }
        public byte? Status { get; set; }
        public string? WardCode { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Ward? WardCodeNavigation { get; set; }
        public virtual ICollection<ClubPost> ClubPosts { get; set; }
        public virtual ICollection<SoloMatch> SoloMatches { get; set; }
        public virtual ICollection<Table> Tables { get; set; }
        public virtual ICollection<Tournament> Tournaments { get; set; }
    }
}
