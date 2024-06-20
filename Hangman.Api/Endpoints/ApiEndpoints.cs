﻿namespace Hangman.Api.Endpoints
{
    public class ApiEndpoints
    {
        private const string ApiBase = "api";

        public const string CreateGame = $"{ApiBase}/games";

        public const string JoinGame = $"/{ApiBase}/games/{{roomCode}}/players";
    }
}
