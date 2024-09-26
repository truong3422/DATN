namespace PoolComVnWebClient.DTO
{
    public class ClubPostDTO
    {
        public int PostId { get; set; }
        public int ClubId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? Link { get; set; }
        public bool? Status { get; set; }
        public string? Flyer { get; set; }

    }
}
