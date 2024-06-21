using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Hangman.Contracts.Requests;
using Hangman.Api.Mapping;
using Hangman.Application.Services;

namespace Hangman.Api.Endpoints
{
    public static class JoinGameEndpoint
    {
        public const string Name = "JoinGame";

        public static IEndpointRouteBuilder MapJoinGame(this IEndpointRouteBuilder app)
        {
            app.MapPost(ApiEndpoints.JoinGame, async (
                [FromRoute] string roomCode, 
                IGameService gameService, 
                CancellationToken token, 
                [FromBody] JoinGameRequest request) =>
            {
                var player = request.MapToPlayer( roomCode );
                await gameService.JoinGameAsync(player, token);
                var response = player.MapToResponse();
                return TypedResults.Ok(response);
            });
            return app;
        }
       
    }
}
