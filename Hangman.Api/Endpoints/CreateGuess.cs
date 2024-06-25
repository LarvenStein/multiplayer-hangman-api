using Hangman.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Hangman.Contracts.Requests;
using Hangman.Api.Mapping;

namespace Hangman.Api.Endpoints
{
    public static class CreateGuess
    {
        public const string name = "CreateGuess";

        public static IEndpointRouteBuilder MapCreateGuess(this IEndpointRouteBuilder app)
        {
            app.MapPost(ApiEndpoints.CreateGuess, async (
                [FromRoute] string roomCode,
                [FromRoute] int round,
                IGameStateService gameStateService,
                CancellationToken token,
                [FromBody] CreateGuessRequest request,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var guess = request.MapToGuess(roomCode, userId, round);
                guess = await gameStateService.HandleGuess(guess);
                return TypedResults.Ok(guess.MapToResponse());
            });
            return app;
        }
    }
}
