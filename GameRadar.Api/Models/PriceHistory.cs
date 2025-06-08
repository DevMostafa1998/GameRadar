namespace GameRadar.Api.Models
{
    public class PriceHistory
    {
        public string GameId { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
}
