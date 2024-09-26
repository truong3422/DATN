namespace PoolComVnWebClient.DTO
{
    public class NewsDTO
    {
       
            public int NewsId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public int AccId { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }
           
            public string? Link { get; set; }
            public bool? Status { get; set; }
           public string? Flyer { get; set; }



    }
}
