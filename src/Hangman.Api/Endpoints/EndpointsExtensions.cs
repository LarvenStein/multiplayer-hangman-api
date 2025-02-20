﻿namespace Hangman.Api.Endpoints
{
    public static class EndpointsExtensions
    {
        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapCreateGame();
            app.MapJoinGame();
            app.MapEditGame();
            app.MapStartGame();
            app.MapGetGameStatus();
            app.MapGetPlayers();
            app.MapGetRoundsStatus();
            app.MapGetRoundStatus();
            app.MapCreateGuess();
            app.MapGetWordlists();
            return app;
        }
    }
}
