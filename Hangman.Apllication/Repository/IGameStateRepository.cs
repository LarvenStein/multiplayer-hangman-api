using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Repository
{
    public interface IGameStateRepository
    {
        Task<int> NextRoundAsync(String gameCode, string word, CancellationToken cancellationToken = default);
        Task<string> GetWordList(String gameCode, CancellationToken cancellationToken = default);
        Task<int> GetCurrentRound(String gameCode, CancellationToken cancellationToken = default);
        Task<GameStatus> GetGame(String gameCode, CancellationToken token = default);
    }
}
