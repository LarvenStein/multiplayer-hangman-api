using Hangman.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Ocsp;
using Hangman.Api.Mapping;
using Hangman.Application.Services;
using Hangman.Application.Models;

namespace Hangman.Api.Hubs
{
    public class GameHub : Hub
    {
        public async Task MakeGuess(string userId, string guess, string roomCode, string round, IGameStateService gameStateService)
        {
            CreateGuessRequest guessReqObj = new CreateGuessRequest { guess = guess };
            var guessObj = guessReqObj.MapToGuess(roomCode, Guid.Parse(userId), Int32.Parse(round));
            guessObj = await gameStateService.HandleGuess(guessObj);
            var roundState = await gameStateService.GetRoundStatus(new RoundStatus { roomCode = roomCode, roundNum = Int32.Parse(round), userId = Guid.Parse(userId) });
            await Clients.Group(roomCode).SendAsync("GameState", roundState.MapToResponse());
        }

        public async Task JoinGame(string roomCode, string nickname, IGameService gameService, IUserService userService)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            Player playerObject = new Player { Id = Guid.NewGuid(), Nickname = nickname, roomCode = roomCode };
            var player = await gameService.JoinGameAsync(playerObject);
            await Clients.Caller.SendAsync("PlayerData", playerObject.MapToResponse());
            
            var players = await gameService.GetAllPlayers(roomCode, playerObject.Id);
            bool isGameLeader = await userService.IsUserGameLeader(roomCode, playerObject.Id);

            await Clients.Group(roomCode).SendAsync("NewPlayer", players.MapToResponse(isGameLeader));
        }

        public async Task StartGame(string roomCode, Guid userid, IGameService gameService)
        {
            var round = await gameService.StartGameAsync(roomCode, userid);
            await Clients.Group(roomCode).SendAsync("GameStarted", round.MapToResponse());
        }

    }
}
