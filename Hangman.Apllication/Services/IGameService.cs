using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Services
{
    public interface IGameService
    {
        Task<String?> CreateGameAsync(CancellationToken token = default);
        Task<bool> JoinGameAsync(Player player, CancellationToken token = default);
        Task<bool> EditGameAsync(GameSettings gameSettings, Guid userId, CancellationToken token = default);
        Task<IEnumerable<string>> GetAllPlayers(string roomCode, Guid userId,  CancellationToken token = default);
        Task<IEnumerable<Wordlist>> GetWordlists(CancellationToken token = default);
    }
}
