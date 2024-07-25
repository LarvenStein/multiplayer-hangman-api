using Hangman.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Ocsp;
using Hangman.Api.Mapping;
using Hangman.Application.Services;
using Hangman.Application.Models;
using System.Security.Claims;

namespace Hangman.Api.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        public GameHub(IGameService gameService, IUserService userService)
        {
            _gameService = gameService;
            _userService = userService;
        }

        public async Task MakeGuess(string userId, string guess, string roomCode, string round, IGameStateService gameStateService)
        {
            CreateGuessRequest guessReqObj = new CreateGuessRequest { guess = guess };
            var guessObj = guessReqObj.MapToGuess(roomCode.ToUpper(), Guid.Parse(userId), Int32.Parse(round));
            guessObj = await gameStateService.HandleGuess(guessObj);
            var roundState = await gameStateService.GetRoundStatus(new RoundStatus { roomCode = roomCode.ToUpper(), roundNum = Int32.Parse(round), userId = Guid.Parse(userId) });
            await Clients.Group(roomCode.ToUpper()).SendAsync("GameState", roundState.MapToResponse());
        }

        public async Task JoinGame(string roomCode, string nickname, IGameService gameService, IUserService userService)
        {
            Player playerObject = new Player { Id = Guid.NewGuid(), Nickname = nickname, roomCode = roomCode.ToUpper() };
            var player = await gameService.JoinGameAsync(playerObject);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode.ToUpper());

            // Adding the user guid to identity to identify user on disconnect
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, playerObject.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.GroupSid, roomCode.ToUpper()));
            var identity = new ClaimsIdentity(claims);
            Context.User!.AddIdentity(identity);

            await Clients.Caller.SendAsync("PlayerData", playerObject.MapToResponse());
            
            var players = await gameService.GetAllPlayers(roomCode.ToUpper(), playerObject.Id);
            bool isGameLeader = await userService.IsUserGameLeader(roomCode.ToUpper(), playerObject.Id);

            await Clients.Group(roomCode.ToUpper()).SendAsync("NewPlayer", players.MapToResponse(isGameLeader));
        }

        public async Task StartGame(string roomCode, Guid userid, IGameService gameService)
        {
            var round = await gameService.StartGameAsync(roomCode.ToUpper(), userid);
            await Clients.Group(roomCode.ToUpper()).SendAsync("GameStarted", round.MapToResponse());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Guid userId = Guid.Parse(Context.User!.Identities.ToList()[1].Name!);
            string roomCode = Context.User!.Identities.ToList()[1].Claims.ToList()[1].ToString().Split(": ")[1].ToUpper();

            // Remove player
            await _userService.RemovePlayer(roomCode, userId);

            await Clients.Group(roomCode).SendAsync("RefreshPlayers");

        }

    }
}
