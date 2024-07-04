using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Application.Repository;
using Hangman.Application.Services;
using Xunit.Abstractions;
using NSubstitute;
using FluentValidation;
using Hangman.Application.Models;
using Hangman.Application.Validators;
using FluentAssertions;
using NSubstitute.ExceptionExtensions;

namespace Hangman.Application.Tests.Unit
{
    public class GameServiceTests 
    {
        private readonly IGameService _sut;
        private readonly IGameReopsitory _gameReopsitory = Substitute.For<IGameReopsitory>();
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly IGameStateRepository _gameStateRepository = Substitute.For<IGameStateRepository>();
        private readonly IRandomService _randomService = new RandomService();
        private readonly IValidator<Player> _playerValidator = new PlayerValidator();
        private readonly IValidator<GameSettings> _gameSettingsValidator = new GameSettingsValidator();
        private readonly ITestOutputHelper _outputHelper;

        public GameServiceTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new GameService(_gameReopsitory, _userRepository, _gameStateRepository, _playerValidator, _gameSettingsValidator, _randomService);
        }

        [Fact]
        public async Task CreateGameAsync_ShouldCreateGame()
        {
            // Arrange 
            _gameReopsitory.CreateGameAsync(Arg.Any<string>()).Returns(true);

            // Act 
            var roomCode = await _sut.CreateGameAsync();

            // Assert
            await _gameReopsitory.Received(1).CreateGameAsync(Arg.Any<string>());
            
        }

        [Fact]
        public async Task CreateGameAsync_ShouldThrowError_When10RoomCodeTriesExceeded()
        {
            // Arrange
            _gameReopsitory.CreateGameAsync(Arg.Any<string>()).Throws<Exception>();

            // Act
            Func<Task> roomCode = async () => { await _sut.CreateGameAsync(); };

            // Assert
            await roomCode.Should()
                .ThrowAsync<Exception>()
                .WithMessage("500;Game could not be created");
        }

        [Theory]
        [InlineData("123", 1, 5)]
        [InlineData("123", 3, 5)]
        [InlineData("123", 14, 15)]
        [InlineData("JustAUserName", 1, 2)]
        [InlineData("ThisIsA25CharLongUsername", 24, 25)]
        public async Task JoinGameAsync_ShouldOnlyCreateUser_WhenRoomIsNotFull(string nickname, int players, int maxPlayers)
        {
            // Arrange
            string roomCode = "AAAAAA";
            _userRepository.GetGameLeader(Arg.Any<string>()).Returns(Guid.NewGuid());

            _gameReopsitory.GetAllPlayers(Arg.Any<string>()).Returns(new string[players]);

            _gameStateRepository.GetGame(Arg.Any<string>()).Returns(new GameStatus
            {
                maxPlayers = maxPlayers,
                roomCode = roomCode,
                round = 0,
                rounds = 5,
                status = "Lobby",
                wordList = "English"
            });

            var playerObject = new Player { Id = Guid.NewGuid(), Nickname = nickname, roomCode = roomCode };

            // Act
            await _sut.JoinGameAsync(playerObject);

            // Assert
            await _userRepository.Received(0).SetGameLeader(Arg.Any<Player>());
            await _gameReopsitory.Received(1).JoinGameAsync(playerObject);
        }

        [Theory]
        [InlineData("123", 5)]
        [InlineData("123", 15)]
        [InlineData("JustAUserName", 1)]
        [InlineData("ThisIsA25CharLongUsername", 25)]
        public async Task JoinGameAsync_ShouldCreateUserAndSetLeader_WhenRoomIsEmpty(string nickname, int maxPlayers)
        {
            // Arrange
            string roomCode = "AAAAAA";
            int players = 0;

            _gameReopsitory.GetAllPlayers(Arg.Any<string>()).Returns(new string[players]);

            _gameStateRepository.GetGame(Arg.Any<string>()).Returns(new GameStatus
            {
                maxPlayers = maxPlayers,
                roomCode = roomCode,
                round = 0,
                rounds = 5,
                status = "Lobby",
                wordList = "English"
            });

            var playerObject = new Player { Id = Guid.NewGuid(), Nickname = nickname, roomCode = roomCode };

            // Act
            await _sut.JoinGameAsync(playerObject);

            // Assert
            await _userRepository.Received(1).SetGameLeader(Arg.Any<Player>());
            await _gameReopsitory.Received(1).JoinGameAsync(playerObject);
        }

        [Theory]
        // The second value CANNOT be higher than the first
        [InlineData("123", 3, 2)]
        [InlineData("123", 14, 14)]
        [InlineData("JustAUserName", 1, 1)]
        [InlineData("ThisIsA25CharLongUsername", 25, 25)]
        public async Task JoinGameAsync_ShouldThrowError_WhenRoomIsFull(string nickname, int players, int maxPlayers)
        {
            // Arrange
            string roomCode = "AAAAAA";
            _userRepository.GetGameLeader(Arg.Any<string>()).Returns(Guid.NewGuid());

            _gameReopsitory.GetAllPlayers(Arg.Any<string>()).Returns(new string[players]);

            _gameStateRepository.GetGame(Arg.Any<string>()).Returns(new GameStatus
            {
                maxPlayers = maxPlayers,
                roomCode = roomCode,
                round = 0,
                rounds = 5,
                status = "Lobby",
                wordList = "English"
            });

            var playerObject = new Player { Id = Guid.NewGuid(), Nickname = nickname, roomCode = roomCode };

            // Act
            Func<Task> joinGame = async () => { await _sut.JoinGameAsync(playerObject); };

            // Assert
            await _userRepository.Received(0).SetGameLeader(Arg.Any<Player>());
            await _gameReopsitory.Received(0).JoinGameAsync(playerObject);
            await joinGame.Should()
                .ThrowAsync<Exception>()
                .WithMessage("403;Game already full :(");
        }

        [Fact]
        public async Task EditGameAsync_ShouldEditGameSettings_WhenUserIsGameLeaderAndSettingsValid()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            string roomCode = "AAAAAA";
            var gameSettings = new GameSettings { gameLeader = guid, maxPlayers = 5, roomCode = roomCode, rounds = 5, wordList = 1 };
            _userRepository.GetGameLeader(roomCode).Returns(guid);
            _gameReopsitory.EditGameAsync(gameSettings).Returns(true);

            // Act
            await _sut.EditGameAsync(gameSettings, guid);

            // Assert
            await _gameReopsitory.Received(1).EditGameAsync(gameSettings);
        }

        [Fact]
        public async Task EditGameAsync_ShouldThrowUnauthorized_WhenUserNotGameLeaderAndSettingsValid()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            string roomCode = "AAAAAA";
            var gameSettings = new GameSettings { gameLeader = guid, maxPlayers = 5, roomCode = roomCode, rounds = 5, wordList = 1 };
            _userRepository.GetGameLeader(roomCode).Returns(Guid.NewGuid());
            _gameReopsitory.EditGameAsync(gameSettings).Returns(true);

            // Act
            Func<Task> editGame = async () => { await _sut.EditGameAsync(gameSettings, guid); };

            // Assert
            await _gameReopsitory.Received(0).EditGameAsync(gameSettings);
            await editGame.Should()
                .ThrowAsync<Exception>()
                .WithMessage("401;Unauthorized");

        }

        [Theory]
        [InlineData(1, "a", 1, 1)]
        [InlineData(1, "aaaaaa", 26, 1)]
        [InlineData(0, "a", 1, 1)]
        [InlineData(1, "a", 1, null)]
        public async Task EditGameAsync_ShouldThrowError_WhenSettingsNotValid(int maxPlayers, string roomCode, int rounds, int wordList)
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var gameSettings = new GameSettings { gameLeader = guid, maxPlayers = maxPlayers, roomCode = roomCode, rounds = rounds, wordList = wordList };
            _userRepository.GetGameLeader(roomCode).Returns(Guid.NewGuid());
            _gameReopsitory.EditGameAsync(gameSettings).Returns(true);

            // Act
            Func<Task> editGame = async () => { await _sut.EditGameAsync(gameSettings, guid); };

            // Assert
            await _gameReopsitory.Received(0).EditGameAsync(gameSettings);
            await editGame.Should()
                .ThrowAsync<ValidationException>();
        }

        [Fact]
        public void GetAllPlayers_SouldReturnUnauthorized_WhenUserNotInRoom()
        {
            // Arrange
            string roomCode = "AAAAAA";
            _gameReopsitory.GetAllPlayers(roomCode).Returns(Enumerable.Empty<string>());

            // Act
            Func<Task> players = async () => { await _sut.GetAllPlayers(roomCode, Guid.NewGuid()); };

            // Assert
            players.Should()
                .ThrowAsync<Exception>()
                .WithMessage("401;Unauthorized");

        }

        [Fact]
        public async Task GetAllPlayers_SouldThrowError_WhenRoomCodeInvalid()
        {
            // Arrange
            string roomCode = "AAAAA";
            _gameReopsitory.GetAllPlayers(roomCode).Returns(Enumerable.Empty<string>());

            // Act
            Func<Task> players = async () => { await _sut.GetAllPlayers(roomCode, Guid.NewGuid()); };

            // Assert
            await _gameReopsitory.Received(0).GetAllPlayers(roomCode);

            await players.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Invalid room code");
        }

        [Theory]
        [InlineData(new object[] { new string[] { "player1" } })]
        [InlineData(new object[] { new string[] { "player1", "player2" } })]
        public async Task GetAllPlayers_ShouldReturnListOfPlayers_WhenUsersInRoom(string[] expectedPlayers)
        {
            // Arrange
            Guid playerGuid = Guid.NewGuid();
            string roomCode = "AAAAAA";

            _gameReopsitory.GetAllPlayers(roomCode).Returns(expectedPlayers);
            _userRepository.GetUserGame(playerGuid).Returns(roomCode);

            // Act
            var players = await _sut.GetAllPlayers(roomCode, playerGuid);

            // Assert
            players.Should().BeEquivalentTo(expectedPlayers);
        }

        [Theory]
        [InlineData(new object[] { new string[] { "word1" } })]
        [InlineData(new object[] { new string[] { "word1", "word2" } })]
        [InlineData(new object[] { new string[] { "word", "word", "word", "word", "word" } })]
        public async Task StartGameAsync_ShouldDeleteAndCreateRoundsAndGetWords(string[] words)
        {
            // Arrange
            var gameStatus = new GameStatus
            {
                rounds = 1,
                maxPlayers = 1,
                roomCode = "AAAAAA",
                round = 0,
                status = "lobby",
                wordList = "English"
            };
            Guid guid = Guid.NewGuid();

            _gameStateRepository.GetGame(Arg.Any<string>()).Returns(gameStatus); 
            _gameReopsitory.GetRandomWord(Arg.Any<int>(), Arg.Any<int>()).Returns(words);
            _userRepository.GetGameLeader(Arg.Any<string>()).Returns(guid);

            // Act
            await _sut.StartGameAsync(gameStatus.roomCode, guid);

            // Assert
            await _gameStateRepository.Received(1).DeleteRounds(gameStatus.roomCode);
            await _gameReopsitory.Received(words.Length).CreateRound(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
            await _gameStateRepository.Received(1).NextRoundAsync(gameStatus.roomCode);
        }

        [Fact]
        public async Task StartGameAsync_SouldThrowError_WhenRoomCodeInvalid()
        {
            // Arrange
            string roomCode = "AAAAA";

            // Act
            Func<Task> start = async () => { await _sut.StartGameAsync(roomCode, Guid.NewGuid()); };

            // Assert
            await start.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Invalid room code");
        }

        [Fact]
        public async Task StartGameAsync_ShouldThrowUnauthorized_WhenUserNotGameLeaderAndSettingsValid()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            string roomCode = "AAAAAA";
            _userRepository.GetGameLeader(roomCode).Returns(Guid.NewGuid());

            // Act
            Func<Task> startGame = async () => { await _sut.StartGameAsync(roomCode, guid); };

            // Assert
            await startGame.Should()
                .ThrowAsync<Exception>()
                .WithMessage("401;Unauthorized");

        }
    }
}
