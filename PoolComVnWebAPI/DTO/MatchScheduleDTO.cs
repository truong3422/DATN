namespace PoolComVnWebAPI.DTO
{
    public class MatchScheduleDTO
    {
        public int MatchId { get; set; }
        public int MatchNumber { get; set; }
        public string MatchCode { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // ScheduleStatus = 0 => chưa lên lịch
        // ScheduleStatus = 1 => đã lên lịch nhưng chưa đấu
        // ScheduleStatus = 2 => đã lên bắt đầu trận đấu
        public byte ScheduleStatus { get; set; }
        public int? TableId { get; set; }
        public string? TableName { get; set; }
        public int? WinToMatch { get; set; }
        public int? LoseToMatch { get; set; }
        public int? RaceTo { get; set; }
        public string? P1Country { get; set; }
        public string? P2Country { get; set; }
        public string? P1Name { get; set; }
        public string? P2Name { get; set; }
    }
}
