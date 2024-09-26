namespace PoolComVnWebAPI.DTO
{
    public class SaveMatchDTO
    {
        public int MatchId { get; set; }
        public int P1Id { get; set; }
        public string? P1GameWin { get; set; }
        public int P1Score { get; set; }
        public int? P1Status { get; set; }
        public int P2Id { get; set; }
        public string? P2GameWin { get; set; }
        public int P2Score { get; set; }
        public int? P2Status { get; set; }
    }
}
