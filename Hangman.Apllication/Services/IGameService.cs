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
    }
}
