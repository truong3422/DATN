namespace PoolComVnWebClient.DTO
{
   
        public class PlayerInSoloMatchDTO
        {
            public int PlayerSoloMatchId { get; set; }
            public int SoloMatchId { get; set; }
            public int PlayerId { get; set; }
            public int? Score { get; set; }
            public string? GameWin { get; set; }
            public bool? IsWinner { get; set; }
        }
    
}
