using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Repository
{
    public interface IGameReopsitory
    {
        Task<bool> CreateGameAsync(String gameCode, CancellationToken token = default);
        Task<bool> JoinGameAsync(Player player, CancellationToken token = default);
        Task<Guid?> GetGameLeader(String gameCode, CancellationToken cancellationToken = default);
        Task<bool> SetGameLeader(Player player, CancellationToken cancellationToken = default);
        Task<bool> EditGameAsync(GameSettings gameSettings, CancellationToken cancellationToken = default);
        Task<string?> GetUserGame(Guid userId);
        Task<int> NextRoundAsync(String gameCode, string word, CancellationToken cancellationToken = default);
        Task<string> GetWordList(String gameCode, CancellationToken cancellationToken = default);
        Task<int> GetCurrentRound(String gameCode, CancellationToken cancellationToken = default);
        Task<GameStatus> GetGame(String gameCode, CancellationToken token = default);
    }
}
