using FluentAssertions;
using Hangman.Application.Models;
using Hangman.Application.Repository;
using Hangman.Application.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly IUserService _sut;
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

        public UserServiceTests()
        {
            _sut = new UserService(_userRepository);
        }

        [Fact]
        public async Task IsUserGameLeader_GS_ShouldReturnTrue_WhenUserIsGameLeader()
        {
            // Arrange
            GameSettings gsObject = new GameSettings { gameLeader = Guid.NewGuid(), maxPlayers = 5, roomCode = "AAAAAA", rounds = 2, wordList= 1 };
            _userRepository.GetGameLeader(gsObject.roomCode).Returns(gsObject.gameLeader);

            // Act
            var result = await _sut.IsUserGameLeader(gsObject, gsObject.gameLeader);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserGameLeader_GS_ShouldReturnFalse_WhenUserIsNotGameLeader()
        {
            // Arrange
            GameSettings gsObject = new GameSettings { gameLeader = Guid.NewGuid(), maxPlayers = 5, roomCode = "AAAAAA", rounds = 2, wordList = 1 };
            _userRepository.GetGameLeader(gsObject.roomCode).Returns(Guid.NewGuid());

            // Act
            var result = await _sut.IsUserGameLeader(gsObject, gsObject.gameLeader);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserGameLeader_ShouldReturnTrue_WhenUserIsGameLeader()
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _userRepository.GetGameLeader(roomCode).Returns(guid);

            // Act
            var result = await _sut.IsUserGameLeader(roomCode, guid);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserGameLeader_ShouldReturnFalse_WhenUserIsNotGameLeader()
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _userRepository.GetGameLeader(roomCode).Returns(Guid.NewGuid());

            // Act
            var result = await _sut.IsUserGameLeader(roomCode, guid);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUserIsUserInGame_ShouldReturnTrue_WhenUserIsInGame()
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _userRepository.GetUserGame(guid).Returns(roomCode);

            // Act
            var result = await _sut.IsUserInGame(roomCode, guid);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserIsUserInGame_ShouldReturnFalse_WhenUserIsNotInGame()
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _userRepository.GetUserGame(guid).Returns("BBBBBB");

            // Act
            var result = await _sut.IsUserInGame(roomCode, guid);

            // Assert
            result.Should().BeFalse();
        }

    }
}
