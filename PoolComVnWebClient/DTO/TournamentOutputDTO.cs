namespace PoolComVnWebClient.DTO
{
    public class TournamentOutputDTO
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        public string ClubName { get; set; }
        public string? ClubAvt { get; set; }
        public int Status { get; set; }
        public string Address { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string GameType { get; set; }
        public string Description { get; set; }
        public string Flyer { get; set; }
    }
}
