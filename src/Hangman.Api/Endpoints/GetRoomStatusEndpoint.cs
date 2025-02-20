﻿using Hangman.Api.Mapping;
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
                IGameStateService gameStateService,
                CancellationToken token,
                [FromHeader(Name = "x-user-id")] Guid userId) =>
            {
                var data = await gameStateService.GetGameStatus(roomCode, userId, token);
                return TypedResults.Ok(data.MapToResponse());
            });
            return app;
        }
    }
}
