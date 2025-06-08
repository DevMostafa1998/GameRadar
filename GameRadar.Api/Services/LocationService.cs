using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace GameRadar.Api.Services
{
    public class LocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetCountryCodeAsync()
        {
            try
            {
                // Use IP geolocation API
                Console.WriteLine("Location Service: Starting location detection...");
                var response = await _httpClient.GetAsync("https://ipapi.co/json/");
                Console.WriteLine($"Location Service: API Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Location Service: Failed to get location: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Location Service: Error Response: {errorContent}");
                    return "US"; // Default to US if location service fails
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Location Service: Raw Response: {content}");
                
                var location = JsonSerializer.Deserialize<LocationResponse>(content);
                Console.WriteLine($"Location Service: Deserialized Location: {JsonSerializer.Serialize(location)}");
                
                if (location?.CountryCode != null)
                {
                    Console.WriteLine($"Location Service: Detected location - Country: {location.CountryName}, City: {location.City}, Region: {location.Region}, Currency: {location.CurrencyCode}");
                    Console.WriteLine($"Location Service: Using country code: {location.CountryCode}");
                    // Convert to uppercase for consistency
                    return location.CountryCode.ToUpper();
                }
                else
                {
                    Console.WriteLine("Location Service: Failed to get country code from location response");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Location Service: Error getting location: {ex.Message}");
                Console.WriteLine($"Location Service: Stack Trace: {ex.StackTrace}");
            }

            // Return default country code if anything fails
            Console.WriteLine("Location Service: Using default country code: US");
            return "US";
        }
    }

    public class LocationResponse
    {
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? CurrencyCode { get; set; } // Added currency code
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
