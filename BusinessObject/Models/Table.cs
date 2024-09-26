using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Table
    {
        public Table()
        {
            MatchOfTournaments = new HashSet<MatchOfTournament>();
        }

        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int ClubId { get; set; }
        public string TagName { get; set; } = null!;
        public bool Status { get; set; }
        public string Size { get; set; } = null!;
        public string Image { get; set; } = null!;
        public bool? IsScheduling { get; set; }
        public bool? IsUseInTour { get; set; }
        public int? Price { get; set; }

        public virtual Club Club { get; set; } = null!;
        public virtual ICollection<MatchOfTournament> MatchOfTournaments { get; set; }
    }
}
