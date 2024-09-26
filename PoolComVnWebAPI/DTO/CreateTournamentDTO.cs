namespace PoolComVnWebAPI.DTO
{
    public class CreateTourStepOneDTO
    {
        public string TournamentName { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int GameTypeId { get; set; }
        public int TournamentTypeId { get; set; }
        public int MaxPlayerNumber { get; set; }
        public int? KnockoutNumber { get; set; }
        public string RaceNumberString { get; set; }
        public int? EntryFee { get; set; }
        public int? PrizeMoney { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public bool Access { get; set; }
        public byte? Status { get; set; }
    }

    public class CreateTourStepTwoDTO
    {
        public int TourID { get; set; }
        public string Flyer { get; set; }
    }
}
