using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GameRadar.Api.Interfaces;
using GameRadar.Api.Models;
using GameRadar.Api.Services;

namespace GameRadar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly LocationService _locationService;

        public GamesController(IGameService gameService, LocationService locationService)
        {
            _gameService = gameService;
            _locationService = locationService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GamePrice>>> SearchGames([FromQuery] string query, [FromQuery] string? countryCode = null)
        {
            Console.WriteLine($"GamesController: Received search request with query='{query}' and countryCode='{countryCode}'");
            try
            {
                var country = countryCode ?? await _locationService.GetCountryCodeAsync();
                var results = await _gameService.SearchGamesAsync(query, country);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching games: {ex.Message}");
            }
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<GamePrice>> GetGamePrices(string gameId)
        {
            try
            {
                var result = await _gameService.GetGamePricesAsync(gameId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting game prices: {ex.Message}");
            }
        }

        [HttpGet("{gameId}/history")]
        public async Task<ActionResult<IEnumerable<PriceHistory>>> GetPriceHistory(string gameId)
        {
            try
            {
                var history = await _gameService.GetPriceHistoryAsync(gameId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting price history: {ex.Message}");
            }
        }
    }
}
