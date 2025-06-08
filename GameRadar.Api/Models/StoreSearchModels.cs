namespace GameRadar.Api.Models
{
    public class StoreSearchRoot
    {
        public int Total { get; set; }
        public List<StoreSearchItem>? Items { get; set; }
    }

    public class StoreSearchItem
    {
        public string? Type { get; set; } // e.g., "app", "sub", "bundle"
        public int Id { get; set; }
        public string? Name { get; set; }
        public StoreSearchPrice? Price { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("tiny_image")]
        public string? TinyImage { get; set; } // Mapped from tiny_image in JSON
        public string? Capsule { get; set; } // URL for a capsule image (fallback or alternative)
    }

    public class StoreSearchPrice
    {
        public string? Currency { get; set; }
        public int Initial { get; set; } // Price in cents
        public int Final { get; set; } // Price in cents
        [System.Text.Json.Serialization.JsonPropertyName("discount_percent")]
        public int Discount_Percent { get; set; }
    }
}
