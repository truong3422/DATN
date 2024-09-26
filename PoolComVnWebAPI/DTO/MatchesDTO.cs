using BusinessObject.Models;

namespace PoolComVnWebAPI.DTO
{
    public class MatchesDTO
    {

        public MatchesDTO()
        {
            PlayerInMatches = new HashSet<PlayerInMatch>();
        }

        public int MatchId { get; set; }
       


        public virtual ICollection<PlayerInMatch> PlayerInMatches { get; set; }
    }
        
}
