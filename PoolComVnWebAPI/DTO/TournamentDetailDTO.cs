namespace PoolComVnWebAPI.DTO
{
    public class TournamentDetailDTO
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        public int ClubId { get; set; }
        public string ClubAvatar { get; set; }
        public string ClubName { get; set; }
        public int Status { get; set; }
        public string Address { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime RegisterDate { get; set; }
        public string GameType { get; set; }
        public int TourTypeId { get; set; }
        public int MaxPlayer { get; set; }
        public string Description { get; set; }
        public string Flyer { get; set; }
        public List<RaceNumber> RaceWin { get; set; }
        public List<RaceNumber> RaceLose { get; set; }
        public int? TotalPrize { get; set; }
        public bool Access { get; set; }
        public int EntryFee { get; set; }
    }

    public class RaceNumber
    {
        public string Round { get; set; }
        public int GameToWin { get; set; }
    }
}
