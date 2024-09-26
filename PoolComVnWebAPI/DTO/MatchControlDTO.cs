namespace PoolComVnWebAPI.DTO
{
    public class MatchControlDTO
    {
        public int MatchId { get; set; }
        public int MatchNumber { get; set; }
        public string MatchCode { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public byte Status { get; set; }
        public int? TableId { get; set; }
        public string? TableName { get; set; }
        public int? WinToMatch { get; set; }
        public int? LoseToMatch { get; set; }
        public int? RaceTo { get; set; }
        public int? P1Id { get; set; }
        public int? P2Id { get; set; }
        public string? P1Country { get; set; }
        public string? P2Country { get; set; }
        public string? P1Name { get; set; }
        public string? P2Name { get; set; }
        public string? P1Score { get; set; }
        public string? P2Score { get; set; }
        public string? P1GameWin { get; set; }
        public string? P2GameWin { get; set; }
        public int? P1Status { get; set; }
        public int? P2Status { get; set; }
    }
}
