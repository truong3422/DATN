namespace PoolComVnWebClient.DTO
{
    public class ManagerDTO
    {
        public IEnumerable<AccountDTO> Accounts { get; set; }
        public IEnumerable<ClubDTO> Clubs { get; set; }
        public IEnumerable<UserDTO> Users { get; set; }

        public PaginatedList<AccountDTO> PaginatedManagerAccounts { get; set; }
        public PaginatedList<ClubDTO> PaginatedClubAccounts { get; set; }
        public PaginatedList<UserDTO> PaginatedUserAccounts { get; set; }
    }
}
