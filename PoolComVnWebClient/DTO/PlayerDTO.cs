using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolComVnWebClient.DTO
{
    public class PlayerDTO
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = null!;
        public int? CountryId { get; set; }
        public int? Level { get; set; }
        public int? UserId { get; set; }
        public int? TourId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool? IsPayed { get; set; }
        public int? Status { get; set; }
        public string? CountryName { get; set; }
        public string? Avatar { get; set; }
        public string? Ranking { get; set; }
        public string? Match { get; set; }
        public string? Game { get; set; }
        public int? Streak { get; set; }
        public string? CountryName2 { get; set; }
        public string? StreakMatch { get; set; }
        public string? CountryImage { get; set; }
    }
}
