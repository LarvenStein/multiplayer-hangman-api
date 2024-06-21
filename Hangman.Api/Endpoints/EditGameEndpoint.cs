using Hangman.Api.Mapping;
using Hangman.Application.Services;
using Hangman.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Api.Endpoints
{
    public static class EditGameEndpoint
    {
        public const string name = "EditGame";

        public static IEndpointRouteBuilder MapEditGame(this IEndpointRouteBuilder app)
        {
            app.MapPut(ApiEndpoints.EditGame, async (
                [FromRoute] string roomCode,
                IGameService gameService,
                CancellationToken token,
                [FromBody] EditGameRequest request,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var gameSettings = request.MapToGameSettings(roomCode);

                var auth = await gameService.EditGameAsync(gameSettings, userId, token);

                var result = gameSettings.MapToResponse();

                return TypedResults.Ok(result);
            });
            return app;
        }
    }
}
