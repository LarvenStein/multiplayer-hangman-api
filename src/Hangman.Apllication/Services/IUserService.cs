using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Services
{
    public interface IUserService
    {
        Task<bool> IsUserGameLeader(GameSettings gameSettings, Guid userId, CancellationToken token = default);
        Task<bool> IsUserGameLeader(String roomCode, Guid userId, CancellationToken token = default);
        Task<bool> IsUserInGame(String roomCode, Guid userId);
    }
}
