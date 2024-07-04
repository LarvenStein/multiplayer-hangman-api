using Hangman.Api.Mapping;
using Hangman.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Api.Endpoints
{
    public static class CreateGameEndpoint
    {
        public const string Name = "CreateGame";
        
        public static IEndpointRouteBuilder MapCreateGame(this IEndpointRouteBuilder app)
        {
            
            app.MapPost(ApiEndpoints.CreateGame, async (CancellationToken token, IGameService gameService) =>
            {
                var roomId = await gameService.CreateGameAsync(token);

                return TypedResults.Created($"/games/{roomId}/players", roomId!.MapToResponse());

            });
            return app;
        }
    }
}
