namespace PoolComVnWebClient.DTO
{
    public class ClubDTO
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Facebook { get; set; }
        public string? Avatar { get; set; }
        public byte? Status { get; set; }
        public int? AccountId { get; set; }
        public string? WardCode { get; set; }

        public string AccountEmail { get; set; }
    }
}
