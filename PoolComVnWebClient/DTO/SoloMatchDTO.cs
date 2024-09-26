namespace PoolComVnWebClient.DTO
{
    public class SoloMatchDTO
    {
        public int SoloMatchId { get; set; }
        public DateTime StartTime { get; set; }
        public int GameTypeId { get; set; }
        public int ClubId { get; set; }
        public string Description { get; set; } = null!;
        public byte Status { get; set; }
        public string? Flyer { get; set; }
        public int? RaceTo { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
