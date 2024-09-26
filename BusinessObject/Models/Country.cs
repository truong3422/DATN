using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Country
    {
        public Country()
        {
            Players = new HashSet<Player>();
        }

        public int CountryId { get; set; }
        public string CountryName { get; set; } = null!;
        public string CountryImage { get; set; } = null!;

        public virtual ICollection<Player> Players { get; set; }
    }
}
