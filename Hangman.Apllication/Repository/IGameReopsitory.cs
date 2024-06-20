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
    }
}
