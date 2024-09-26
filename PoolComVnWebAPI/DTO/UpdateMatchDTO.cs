namespace PoolComVnWebAPI.DTO
{
    public class UpdateMatchDTO
    {
        public int MatchId { get; set; }
        public int? P1Id { get; set; }
        public int? P2Id { get; set; }
        public string? P1Score { get; set; }
        public string? P2Score { get; set; }
        public string? P1GameWin { get; set; }
        public string? P2GameWin { get; set; }
    }
}
