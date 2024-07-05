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
        Task<bool> EditGameAsync(GameSettings gameSettings, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetAllPlayers(string roomCode, CancellationToken token = default);
        Task<IEnumerable<Wordlist>> GetWordlists(CancellationToken token = default);
        Task<IEnumerable<string>> GetRandomWord(int wordList, int words);
        Task<bool> CreateRound(string roomCode, string word, int roundNum,  CancellationToken token = default);
        Task DeleteGame(string roomCode);
        Task NewGameLeader(string roomCode);
    }
}
