using Hangman.Api.Mapping;
using Hangman.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Api.Endpoints
{
    public static class StartGameEndpoint
    {
        public const string name = "startGame";
        public static IEndpointRouteBuilder MapStartGame(this IEndpointRouteBuilder app)
        {
            app.MapPost(ApiEndpoints.StartGame, async (
                [FromRoute] string roomCode,
                IGameService gameService,
                CancellationToken token,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var result = await gameService.NextRoundAsync(roomCode, userId, true);
                return TypedResults.Ok(result.MapToResponse());
            });
            return app;
        }
    }
}
