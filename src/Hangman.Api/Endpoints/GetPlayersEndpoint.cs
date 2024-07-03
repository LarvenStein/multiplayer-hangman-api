using Hangman.Api.Mapping;
using Hangman.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Hangman.Api.Endpoints
{
    public static class GetPlayersEndpoint
    {
        public const string name = "GetRoomPlayers";

        public static IEndpointRouteBuilder MapGetPlayers(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.GetPlayers, async (
                [FromRoute] string roomCode,
                IGameService gameService,
                IUserService userService,
                IGameStateService gameStateService,
                CancellationToken token,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var result = await gameService.GetAllPlayers(roomCode, userId);
                bool isGameLeader = await userService.IsUserGameLeader(roomCode, userId);
                return result.MapToResponse(isGameLeader);
            });
            return app;
        }
    }
}
