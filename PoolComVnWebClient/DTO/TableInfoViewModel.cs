namespace PoolComVnWebClient.DTO
{
    public class TableInfoViewModel
    {
        public string TableName { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string Image { get; set; } = null!;
        public int? Price { get; set; }
        public int Quantity { get; set; }
    }
}
