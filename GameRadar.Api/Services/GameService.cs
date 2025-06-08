using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using GameRadar.Api.Models;
using GameRadar.Api.Interfaces;
using System.Text.Json.Serialization;

namespace GameRadar.Api.Services
{
    public class GameService : IGameService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly LocationService _locationService;

        public GameService(HttpClient httpClient, IConfiguration configuration, IDistributedCache cache, LocationService locationService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
            _locationService = locationService;

            // Configure HTTP client
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<GamePrice>> SearchGamesAsync(string query, string? countryCode = null)
        {
            Console.WriteLine($"GameService.SearchGamesAsync: Called with query='{query}', provided countryCode='{countryCode}'");
            // If no countryCode provided, get it from location service
            if (string.IsNullOrEmpty(countryCode))
            {
                countryCode = await _locationService.GetCountryCodeAsync();
                Console.WriteLine($"GameService.SearchGamesAsync: Using country code from location service: {countryCode}");
            }
            var results = new List<GamePrice>();
            
            try
            {
                // Search on Steam
                var steamResults = await SearchSteamAsync(query, countryCode);
                results.AddRange(steamResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching games: {ex.Message}");
            }

            return results;
        }

        public async Task<IEnumerable<GamePrice>> GetGamePricesAsync(string gameId)
        {
            // For now, we assume gameId is a Steam App ID and we only fetch from Steam.
            string cacheKey = $"steam_appdetails_{gameId}";
            string? cachedData = null;

            try
            {
                cachedData = await _cache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing cache for key {cacheKey}: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(cachedData))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                var cachedResult = JsonSerializer.Deserialize<List<GamePrice>>(cachedData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return cachedResult ?? new List<GamePrice>();
            }

            Console.WriteLine($"Cache miss for {cacheKey}. Fetching from Steam API.");
            var results = new List<GamePrice>();
            try
            {
                // Steam AppDetails API: https://wiki.teamfortress.com/wiki/User:RJackson/StorefrontAPI#appdetails
                // Example: https://store.steampowered.com/api/appdetails?appids=440&cc=us&l=en
                var appDetailsUrl = $"https://store.steampowered.com/api/appdetails?appids={gameId}&cc=us&l=en";
                var response = await _httpClient.GetAsync(appDetailsUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to get app details from Steam for appid {gameId}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return results;
                }

                var content = await response.Content.ReadAsStringAsync();
                // The response is a dictionary where the key is the app ID
                var appDetailsRoot = JsonSerializer.Deserialize<Dictionary<string, SteamAppDetailsContainer>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip });

                if (appDetailsRoot != null && appDetailsRoot.TryGetValue(gameId, out var appContainer) && appContainer.Success && appContainer.Data != null)
                {
                    var gameData = appContainer.Data;
                    if (gameData.Price_Overview != null)
                    {
                        var gamePrice = new GamePrice
                        {
                            Id = gameData.Steam_Appid.ToString(),
                            Title = gameData.Name ?? "N/A",
                            CurrentPrice = gameData.Price_Overview.Final / 100.0m,
                            OriginalPrice = gameData.Price_Overview.Initial / 100.0m,
                            Discount = gameData.Price_Overview.Discount_Percent,
                            Platform = "Steam",
                            Store = "Steam",
                            Link = $"https://store.steampowered.com/app/{gameData.Steam_Appid}",
                            ImageUrl = gameData.Header_Image
                        };
                        results.Add(gamePrice);
                    }
                    else if (gameData.Is_Free)
                    {
                         var gamePrice = new GamePrice
                        {
                            Id = gameData.Steam_Appid.ToString(),
                            Title = gameData.Name ?? "N/A",
                            CurrentPrice = 0m,
                            OriginalPrice = 0m,
                            Discount = 0,
                            Platform = "Steam",
                            Store = "Steam",
                            Link = $"https://store.steampowered.com/app/{gameData.Steam_Appid}",
                            ImageUrl = gameData.Header_Image
                        };
                        results.Add(gamePrice);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Steam app details for appid {gameId}: {ex.Message}\n{ex.StackTrace}");
            }

            // Store in cache
            try
            {
                if (results.Any())
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Cache for 1 hour
                    };
                    var jsonData = JsonSerializer.Serialize(results);
                    await _cache.SetStringAsync(cacheKey, jsonData, options);
                    Console.WriteLine($"Stored app details in cache: {cacheKey}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing app details data in cache for key {cacheKey}: {ex.Message}");
            }

            return results;
        }

        public async Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string gameId)
        {
            // TODO: Implement price history fetching logic
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<GamePrice>> SearchSteamAsync(string term, string countryCode)
        {
            Console.WriteLine($"GameService.SearchSteamAsync: Called with term='{term}', countryCode='{countryCode}'");
            string cacheKey = $"steam_search_{term.ToLower().Replace(" ", "_")}_{countryCode.ToLower()}";
            Console.WriteLine($"GameService.SearchSteamAsync: Using cacheKey='{cacheKey}'");
            string? cachedData = null;

            try
            {
                cachedData = await _cache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing cache for key {cacheKey}: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(cachedData))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return JsonSerializer.Deserialize<List<GamePrice>>(cachedData) ?? new List<GamePrice>();
            }

            Console.WriteLine($"Cache miss for {cacheKey}. Fetching from Steam API.");
            var results = new List<GamePrice>();
            try
            {
                var searchUrl = $"https://store.steampowered.com/api/storesearch/?cc={countryCode}&l=en&term={term}&cc={countryCode}";
                var response = await _httpClient.GetAsync(searchUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to get search results from Steam for term '{term}': {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return results;
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GameService.SearchSteamAsync: API Response: {content}");

                var searchRoot = JsonSerializer.Deserialize<StoreSearchRoot>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                });

                if (searchRoot == null || searchRoot.Items == null)
                {
                    Console.WriteLine($"GameService.SearchSteamAsync: Invalid response format");
                    return results;
                }

                foreach (var item in searchRoot.Items)
                {
                    if (item.Type == "app" && item.Price != null)
                    {
                        var gamePrice = new GamePrice
                        {
                            Id = item.Id.ToString(),
                            Title = item.Name,
                            CurrentPrice = (decimal)item.Price.Final / 100.0m,
                            OriginalPrice = (decimal)item.Price.Initial / 100.0m,
                            Discount = item.Price.Initial > item.Price.Final 
                                ? (int)Math.Round(((item.Price.Initial - item.Price.Final) / (decimal)item.Price.Initial) * 100)
                                : 0,
                            Platform = "Steam",
                            Store = "Steam",
                            Link = $"https://store.steampowered.com/app/{item.Id}",
                            ImageUrl = item.TinyImage ?? item.Capsule,
                            CurrencyCode = item.Price.Currency
                        };
                        Console.WriteLine($"GameService: Game '{item.Name}' price info - Current: {gamePrice.CurrentPrice} {gamePrice.CurrencyCode}, Original: {gamePrice.OriginalPrice} {gamePrice.CurrencyCode}, Discount: {gamePrice.Discount}%");
                        results.Add(gamePrice);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching Steam store for term '{term}': {ex.Message}");
                throw; // Let the exception propagate to be handled by the caller
            }

            return results;
        }



    private class StoreSearchItem
    {
        public string? Type { get; set; } // e.g., "app", "sub", "bundle"
        public int Id { get; set; }
        public string? Name { get; set; }
        public StoreSearchPrice? Price { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("tiny_image")]
        public string? TinyImage { get; set; } // Mapped from tiny_image in JSON
        public string? Capsule { get; set; } // URL for a capsule image (fallback or alternative)
        // public List<string> Platforms { get; set; } // e.g., ["windows", "mac", "linux"]
        // There are other fields like 'controller_support', 'streamingvideo', etc.
    }

    private class StoreSearchPrice
    {
        public string? Currency { get; set; }
        public int Initial { get; set; } // Price in cents
        public int Final { get; set; } // Price in cents
        [System.Text.Json.Serialization.JsonPropertyName("discount_percent")]
        public int Discount_Percent { get; set; }
        // Individual_formatted and initial_formatted might also be available
    }

    // Helper classes for Steam AppDetails API deserialization
    private class SteamAppDetailsContainer
    {
        public bool Success { get; set; }
        public SteamAppData? Data { get; set; }
    }

    private class SteamAppData
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("steam_appid")]
        public int Steam_Appid { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("is_free")]
        public bool Is_Free { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("detailed_description")]
        public string? Detailed_Description { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("about_the_game")]
        public string? About_The_Game { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("short_description")]
        public string? Short_Description { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("header_image")]
        public string? Header_Image { get; set; }
        // public List<string> Developers { get; set; }
        // public List<string> Publishers { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("price_overview")]
        public SteamPriceOverview? Price_Overview { get; set; }
        // There are many other fields like 'platforms', 'metacritic', 'categories', 'genres', 'screenshots', 'movies', 'release_date', etc.
    }

    private class SteamPriceOverview
    {
        public string? Currency { get; set; }
        public int Initial { get; set; } // Price in cents
        public int Final { get; set; } // Price in cents
        [System.Text.Json.Serialization.JsonPropertyName("discount_percent")]
        public int Discount_Percent { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("initial_formatted")]
        public string? Initial_Formatted { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("final_formatted")]
        public string? Final_Formatted { get; set; }
    }
}
}
