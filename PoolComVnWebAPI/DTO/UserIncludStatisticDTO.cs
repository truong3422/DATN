namespace PoolComVnWebAPI.DTO
{
    public class UserIncludStatisticDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Country { get; set; }
        public string? CountryImage { get; set; }
        public string? Province { get; set; }
        public string Email { get; set; }
        public int TournamentJoined { get; set; }
        public int MatchWin { get; set; }
        public int MatchLose { get; set; }
    }
}
