namespace PoolComVnWebAPI.DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime Dob { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int AccountId { get; set; }
        public string? Address { get; set; }
        public string? WardCode { get; set; }
        public string AccountEmail { get; set; }
        public string PhoneNumber { get; set; }
        public bool Status { get; set; }

    }
}
