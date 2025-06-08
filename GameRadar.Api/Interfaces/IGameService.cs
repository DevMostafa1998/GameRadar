using System.Collections.Generic;
using System.Threading.Tasks;
using GameRadar.Api.Models;

namespace GameRadar.Api.Interfaces
{
    public interface IGameService
    {
        Task<IEnumerable<GamePrice>> SearchGamesAsync(string query, string countryCode);
        Task<IEnumerable<GamePrice>> GetGamePricesAsync(string gameId);
        Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string gameId);
    }
}
