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
        Task<string> GetWord(String gameCode, int round, CancellationToken token = default);
        Task<bool> GuessExsists(Guess guess, CancellationToken token = default);
        Task<int> CountCorrectGuesses(Guess guess, CancellationToken token = default);
        Task<int> CountIncorrectGuesses(Guess guess, CancellationToken token = default);
        Task<bool> MakeGuess(Guess guess, CancellationToken token = default);
        Task<string> GetRoundState(String gameCode, int round, CancellationToken token = default);
        Task<bool> SetRoundState(String roomCode, int roundNum, string newState,  CancellationToken token = default);
    }
}
