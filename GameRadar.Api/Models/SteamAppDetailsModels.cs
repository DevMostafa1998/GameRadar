namespace GameRadar.Api.Models
{
    public class SteamAppDetailsContainer
    {
        public bool Success { get; set; }
        public SteamAppData? Data { get; set; }
    }

    public class SteamAppData
    {
        public string? Name { get; set; }
        public string? HeaderImage { get; set; }
        public string? Background { get; set; }
        public string? Type { get; set; }
        public string? PriceOverview { get; set; }
        public string? Currency { get; set; }
        public int? Initial { get; set; }
        public int? Final { get; set; }
        public int? Discount_Percent { get; set; }
        public string? InitialFormatted { get; set; }
        public string? FinalFormatted { get; set; }
    }
}
