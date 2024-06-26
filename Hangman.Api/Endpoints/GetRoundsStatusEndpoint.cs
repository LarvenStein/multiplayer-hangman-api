using Hangman.Application.Services;
using Hangman.Api.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Api.Endpoints
{
    public static class GetRoundsStatusEndpoint
    {
        public const string name = "RoundsStatus";
        public static IEndpointRouteBuilder MapGetRoundsStatus(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.RoundsStatus, async (
                [FromRoute] string roomCode,
                IGameStateService gameStateService,
                CancellationToken token,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var result = await gameStateService.GetRoundsStatus(roomCode, userId, token);
                return TypedResults.Ok(result.MapToResponse());
            });
            return app;
        }
    }
}
