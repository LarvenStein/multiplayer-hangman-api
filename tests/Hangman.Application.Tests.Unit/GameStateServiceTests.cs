using FluentAssertions;
using FluentValidation;
using Hangman.Application.Models;
using Hangman.Application.Repository;
using Hangman.Application.Services;
using Hangman.Application.Validators;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Hangman.Application.Tests.Unit
{
    public class GameStateServiceTests
    {
        private readonly IGameStateService _sut;
        private readonly IGameReopsitory _gameReopsitory = Substitute.For<IGameReopsitory>();
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly IGameStateRepository _gameStateRepository = Substitute.For<IGameStateRepository>();
        private readonly IValidator<Guess> _guessValidator = new GuessValidator();
        private readonly IValidator<RoundStatus> _roundStatusValidator = new RoundStatusValidator();
        private readonly ITestOutputHelper _outputHelper;

        public GameStateServiceTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new GameStateService(_gameReopsitory, _userRepository, _gameStateRepository, _guessValidator, _roundStatusValidator);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(4, 5)]
        public async Task NextRoundAsync_ShouldCreateNewRound_WhenGameNotFinished(int round, int rounds)
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _gameStateRepository.GetGame(roomCode).Returns(new GameStatus { round = round, rounds = rounds, maxPlayers = 5, roomCode = roomCode, status = "playing", wordList = "English" });
            _gameStateRepository.NextRoundAsync(roomCode).Returns(round + 1);
            // Act
            int nextRound = await _sut.NextRoundAsync(roomCode, guid);

            // Assert
            nextRound.Should().Be(round + 1);
            await _gameStateRepository.Received(1).NextRoundAsync(roomCode);
        }

        [Theory]
        [InlineData(5, 5)]
        [InlineData(6, 5)]
        public async Task NextRoundAsync_ShouldReturnRoundPlus1_WhenGameNotFinished(int round, int rounds)
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _gameStateRepository.GetGame(roomCode).Returns(new GameStatus { round = round, rounds = rounds, maxPlayers = 5, roomCode = roomCode, status = "playing", wordList = "English" });
            _gameStateRepository.NextRoundAsync(roomCode).Returns(round + 1);
            // Act
            int nextRound = await _sut.NextRoundAsync(roomCode, guid);

            // Assert
            nextRound.Should().Be(round + 1);
            await _gameStateRepository.Received(0).NextRoundAsync(roomCode);
        }

        [Fact]
        public async Task NextRoundAsync_SouldThrowError_WhenRoomCodeInvalid()
        {
            // Arrange
            string roomCode = "AAAAA";

            // Act
            Func<Task> nextRound = async () => { await _sut.NextRoundAsync(roomCode, Guid.NewGuid()); };

            // Assert
            await nextRound.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Invalid room code");
        }

        [Fact]
        public async Task GetGameStatus_ShouldReturnGameStatus_WhenValidationSuceeded()
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            var gameState = new GameStatus
            {
                round = 1,
                rounds = 5,
                maxPlayers = 5,
                roomCode = roomCode,
                status = "playing",
                wordList = "English"
            };
            _userRepository.GetUserGame(guid).Returns(roomCode);
            _gameStateRepository.GetGame(roomCode).Returns(gameState);

            // Act
            var gameStatus = await _sut.GetGameStatus(roomCode, guid);

            // Assert
            gameStatus.Should().BeEquivalentTo(gameState);

        }

        [Fact]
        public async Task GetGameStatus_SouldThrowError_WhenRoomCodeInvalid()
        {
            // Arrange
            string roomCode = "AAAAA";

            // Act
            Func<Task> gameStatus = async () => { await _sut.GetGameStatus(roomCode, Guid.NewGuid()); };

            // Assert
            await gameStatus.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Invalid room code");
        }

        [Fact]
        public async Task GetGameStatus_SouldThrowError_WhenUserNotInRoom()
        {
            // Arrange
            string roomCode = "AAAAAA";
            Guid guid = Guid.NewGuid();
            _userRepository.GetUserGame(guid).Returns("BBBBBB");

            // Act
            Func<Task> gameState = async () => { await _sut.GetGameStatus(roomCode, guid); };

            // Assert
            await gameState.Should()
                .ThrowAsync<Exception>()
                .WithMessage("401;Unauthorized");
        }

        [Theory]
        [InlineData("w", "word", 0, 0, true, "active")]
        [InlineData("e", "word", 0, 0, false, "active")]
        [InlineData("d", "word", 0, 4, true, "won")]
        [InlineData("z", "word", 10, 3, false, "lost")]
        [InlineData("thisgameisawsome", "word", 3, 1, false, "active")]
        [InlineData("ding", "word", 10, 3, false, "lost")]
        [InlineData("word", "word", 0, 3, true, "won")]
        public async Task HandleGuess_ShouldBeCheckedAndSaved_WhenRoundIsActive(string guess, string word, int falseGuesses, int correctGuesses, bool correct, string roundState)
        {
            // Arrange 
            var guessObject = new Guess { playerId = Guid.NewGuid(), roomCode = "AAAAAA", guess = guess, roundNum = 1 };
            var expectedGuess = guessObject;
            expectedGuess.correct = correct;
            _userRepository.GetUserGame(guessObject.playerId).Returns(guessObject.roomCode);
            _gameStateRepository.GuessExsists(guessObject).Returns(false);
            _gameStateRepository.GetRoundState(guessObject.roomCode, guessObject.roundNum).Returns("active");
            _gameStateRepository.GetWord(guessObject.roomCode, guessObject.roundNum).Returns(word);
            _gameStateRepository.CountCorrectGuesses(guessObject).Returns(correctGuesses);
            _gameStateRepository.CountIncorrectGuesses(guessObject).Returns(falseGuesses);
            _gameStateRepository.GetGame(guessObject.roomCode).Returns(new GameStatus { maxPlayers = 1, roomCode = guessObject.roomCode, round = guessObject.roundNum, rounds = 99, status = "", wordList = "" }); // holy shit

            // Act
            var makeGuess = await _sut.HandleGuess(guessObject); 

            // Assert
            await _gameStateRepository.Received(1).MakeGuess(Arg.Is<Guess>(p => p.correct == correct));
            await _gameStateRepository.Received(1).SetRoundState(guessObject.roomCode, guessObject.roundNum, Arg.Is(roundState));
            makeGuess.Should().BeEquivalentTo(expectedGuess);

            if (roundState != "active")
            {
                // If tried to create new round
                await _gameStateRepository.Received(1).NextRoundAsync(guessObject.roomCode);
            }
        }

        [Fact]
        public async Task HandleGuess_ShouldThrowError_WhenRoundInactive()
        {
            // This is quite janky but works

            // Arrange
            var guessObject = new Guess { playerId = Guid.NewGuid(), roomCode = "AAAAAA", guess = "A", roundNum = 1 };
            _userRepository.GetUserGame(guessObject.playerId).Returns("BBBBBB");

            // Act
            Func<Task> handleGuess = async () => { await _sut.HandleGuess(guessObject); };

            // Assert
            await handleGuess.Should()
                .ThrowAsync<Exception>()
                .WithMessage("400;Round not active");

        }

        [Fact]
        public async Task HandleGuess_ShouldThrowError_WhenPlayerNotInRound()
        {
            // Arrange
            var guessObject = new Guess { playerId = Guid.NewGuid(), roomCode = "AAAAAA", guess = "A", roundNum = 1 };
            _userRepository.GetUserGame(guessObject.playerId).Returns("BBBBBB");
            _gameStateRepository.GetRoundState(guessObject.roomCode, guessObject.roundNum).Returns("active");

            // Act
            Func<Task> handleGuess = async () => { await _sut.HandleGuess(guessObject); };

            // Assert
            await handleGuess.Should()
                .ThrowAsync<Exception>()
                .WithMessage("401;Unauthorized");
        }

        [Fact]
        public async Task HandleGuess_ShouldThrowError_WhenGuessAlreadyMade()
        {
            // Arrange
            var guessObject = new Guess { playerId = Guid.NewGuid(), roomCode = "AAAAAA", guess = "A", roundNum = 1 };
            _userRepository.GetUserGame(guessObject.playerId).Returns(guessObject.roomCode);
            _gameStateRepository.GetRoundState(guessObject.roomCode, guessObject.roundNum).Returns("active");
            _gameStateRepository.GuessExsists(guessObject).Returns(true);

            // Act
            Func<Task> handleGuess = async () => { await _sut.HandleGuess(guessObject); };

            // Assert
            await handleGuess.Should()
                .ThrowAsync<Exception>()
                .WithMessage("400;Guess was already made");
        }

        [Theory]
        [InlineData("AAAAA", 1, "guess")]
        [InlineData("AAAAAA", null, "guess")]
        [InlineData("AAAAAA", 1, "")]
        public async Task HandleGuess_ShouldThrowError_WhenInputInvalid(string roomCode, int roundNum, string guess)
        {
            // Arrange
            var guessObject = new Guess { playerId = Guid.NewGuid(), roomCode = roomCode, guess = guess, roundNum = roundNum };

            // Act 
            Func<Task> handleGuess = async () => { await _sut.HandleGuess(guessObject); };

            // Assert
            await handleGuess.Should()
                .ThrowAsync<ValidationException>();
        }
    }
}
