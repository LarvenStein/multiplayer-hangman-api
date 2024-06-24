using Hangman.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace Hangman.Api.Endpoints
{
    public static class GetRoomStatusEndpoint
    {
        public const string name = "RoomStatus";
        public static IEndpointRouteBuilder MapGetGameStatus(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.GameStatus, async (
                [FromRoute] string roomCode,
                IGameService gameService,
                CancellationToken token,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var data = await gameService.GetGameStatus(roomCode, userId, token);
                return TypedResults.Ok(data); // TODO: MAP TO RESPONSE
            });
            return app;
        }
    }
}
