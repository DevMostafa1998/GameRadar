namespace GameRadar.Api.Models
{
    public class GamePrice
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Discount { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CurrencyCode { get; set; }
    }
}
