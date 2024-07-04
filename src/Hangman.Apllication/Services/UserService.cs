using Hangman.Application.Models;
using Hangman.Application.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> IsUserGameLeader(GameSettings gameSettings, Guid userId, CancellationToken token = default)
        {
            var gameLeader = await _userRepository.GetGameLeader(gameSettings.roomCode, token);
            return userId == gameLeader;
        }

        public async Task<bool> IsUserGameLeader(String roomCode, Guid userId, CancellationToken token = default)
        {
            var gameLeader = await _userRepository.GetGameLeader(roomCode, token);
            return userId == gameLeader;
        }

        public async Task<bool> IsUserInGame(string roomCode, Guid userId)
        {
            var userGameId = await _userRepository.GetUserGame(userId);
            return (userGameId is not null && userGameId == roomCode);
        }
    }
}
