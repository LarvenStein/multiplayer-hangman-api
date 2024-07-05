using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Repository
{
    public interface IUserRepository
    {
        Task<bool> SetGameLeader(Player player, CancellationToken cancellationToken = default);
        Task<Guid?> GetGameLeader(String gameCode, CancellationToken cancellationToken = default);
        Task<string?> GetUserGame(Guid userId);
        Task DeletePlayer(Guid userId);
    }
}
