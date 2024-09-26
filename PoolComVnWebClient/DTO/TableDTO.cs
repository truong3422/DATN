namespace PoolComVnWebClient.DTO
{
    public class TableDTO
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int ClubId { get; set; }
        public string TagName { get; set; } = null!;
        public bool Status { get; set; }
        public string Size { get; set; } = null!;
        public string Image { get; set; } = null!;
        public bool? IsScheduling { get; set; }
        public bool? IsUseInTour { get; set; }
        public int? Price { get; set; }

    }
}
