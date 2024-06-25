﻿namespace Hangman.Api.Endpoints
{
    public class ApiEndpoints
    {
        private const string ApiBase = "api";

        public const string CreateGame = $"{ApiBase}/games";

        public const string JoinGame = $"/{ApiBase}/games/{{roomCode}}/players";

        public const string EditGame = $"{ApiBase}/games/{{roomCode}}";

        public const string StartGame = $"{ApiBase}/games/{{roomCode}}";

        public const string GameStatus = $"{ApiBase}/games/{{roomCode}}";

        public const string GetPlayers = $"/{ApiBase}/games/{{roomCode}}/players";

    }
}
