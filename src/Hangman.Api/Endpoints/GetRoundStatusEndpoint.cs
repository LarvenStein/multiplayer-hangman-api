using Hangman.Application.Services;
using Hangman.Api.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Api.Endpoints
{
    public static class GetRoundStatusEndpoint
    {
        public const string name = "RoundStatus";
        public static IEndpointRouteBuilder MapGetRoundStatus(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.RoundStatus, async (
                [FromRoute] string roomCode,
                [FromRoute] int round,
                IGameStateService gameStateService,
                CancellationToken token,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var roundStatus = roomCode.MapToRoundStatus(userId, round); // 🥸
                var result = await gameStateService.GetRoundStatus(roundStatus, token);
                return TypedResults.Ok(result.MapToResponse());
            });
            return app;
        }
    }
}
