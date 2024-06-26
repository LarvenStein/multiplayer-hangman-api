using Hangman.Application.Models;
using Hangman.Contracts.Requests;
using Hangman.Contracts.Responses;
using System.Runtime.CompilerServices;
using Hangman.Application.Services;

namespace Hangman.Api.Mapping
{
    public static class ContractMapping
    {
        public static CreateGameResponse MapToResponse(this String roomCode)
        {
            return new CreateGameResponse { roomCode = roomCode };
        }
        public static Player MapToPlayer(this JoinGameRequest request,  String roomCode)
        {
            return new Player
            {
                Id = Guid.NewGuid(),
                Nickname = request.nickname,
                roomCode = roomCode,
            };
        }

        public static JoinGameResponse MapToResponse(this Player player)
        {
            return new JoinGameResponse
            {
                Id = player.Id,
                nickname = player.Nickname,
                roomCode = player.roomCode,
            };
        }

        public static EditGameResponse MapToResponse(this GameSettings gameSettings)
        {
            return new EditGameResponse
            {
                rounds = gameSettings.rounds,
                maxPlayers = gameSettings.maxPlayers,
                wordList = gameSettings.wordList,
            };
        }

        public static GameSettings MapToGameSettings(this EditGameRequest request, String roomCode)
        {
            return new GameSettings
            {
                rounds = request.rounds,
                maxPlayers = request.maxPlayers,
                gameLeader = request.newGameLeader,
                wordList = request.wordList,
                roomCode = roomCode
            };
        }

        public static StartGameResponse MapToResponse(this int roundId)
        {
            return new StartGameResponse
            {
                RoundId = roundId,
            };
        }

        public static RoomStatusResponse MapToResponse(this GameStatus gameStatus)
        {
            return new RoomStatusResponse
            {
                roomCode = gameStatus.roomCode,
                maxPlayers = gameStatus.maxPlayers,
                rounds = gameStatus.rounds,
                wordList = gameStatus.wordList,
                status = gameStatus.status,
                round = gameStatus.round,

            };
        }

        public static GetPlayersResponse MapToResponse(this IEnumerable<string> players, bool gameLeader)
        {
            return new GetPlayersResponse
            {
                players = players,
                isPlayerGameLeader = gameLeader
            };
        }

        public static Guess MapToGuess(this CreateGuessRequest request, string gameCode, Guid userId, int round)
        {
            return new Guess
            {
                playerId = userId,
                roomCode = gameCode,
                roundNum = round,
                guess = request.guess.ToUpper()
            };
        }

        public static CreateGuessResponse MapToResponse(this Guess guess)
        {
            return new CreateGuessResponse
            {
                correct = guess.correct,
                guess = guess.guess,
                roundNum = guess.roundNum
            };
        }

        public static RoundStatus MapToRoundStatus(this string gameCode, Guid userId, int round)
        {
            return new RoundStatus
            {
                roomCode = gameCode,
                roundNum = round,
                userId = userId
            };
        }

        public static GetRoudStatusResponse MapToResponse(this RoundStatus roundStatus)
        {
            return new GetRoudStatusResponse
            {
                roomCode = roundStatus.roomCode,
                roundNum = roundStatus.roundNum,
                status = roundStatus.status!,
                correctGuesses = roundStatus.correctGuesses,
                falseGuesses = roundStatus.falseGuesses,
                lifesLeft = roundStatus.livesLeft,
                guessedWord = roundStatus.guessedWord,
                wrongLetters = roundStatus.wrongLetters!
            };
        }

        public static IEnumerable<GetRoudStatusResponse> MapToResponse(this IEnumerable<RoundStatus> rounds)
        {
            return rounds.Select(x => new GetRoudStatusResponse
            {
                roomCode = x.roomCode,
                roundNum = x.roundNum,
                status = x.status!,
                correctGuesses = x.correctGuesses,
                falseGuesses = x.falseGuesses,
                lifesLeft = x.livesLeft,
                guessedWord = x.guessedWord,
                wrongLetters = x.wrongLetters!
            });
        }
    }
}
