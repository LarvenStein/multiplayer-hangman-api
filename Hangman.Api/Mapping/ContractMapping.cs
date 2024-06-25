using Hangman.Application.Models;
using Hangman.Contracts.Requests;
using Hangman.Contracts.Responses;
using System.Runtime.CompilerServices;
using Hangman.Application.Services;

namespace Hangman.Api.Mapping
{
    public static class ContractMapping
    {
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
    }
}
