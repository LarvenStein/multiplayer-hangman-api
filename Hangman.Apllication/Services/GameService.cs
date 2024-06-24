using FluentValidation;
using Hangman.Application.Models;
using Hangman.Application.Repository;
using Hangman.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Hangman.Application.Services
{
    public class GameService : IGameService
    {
        private readonly IGameReopsitory _gameReopsitory;
        private readonly IValidator<Player> _playerValidator;
        private readonly IValidator<GameSettings> _gameSettingsValidator;

        public GameService(IGameReopsitory gameReopsitory, IValidator<Player> playerValidator, IValidator<GameSettings> settingsValidator)
        {
            _gameReopsitory = gameReopsitory;
            _playerValidator = playerValidator;
            _gameSettingsValidator = settingsValidator;
        }

        public async Task<string?> CreateGameAsync(CancellationToken token = default)
        {
            bool result = false;
            String roomCode = "";
            int tries = 0;
            // Loop until valid room code is found.
            while(!result)
            {
                if(tries > 10)
                {
                    throw new Exception("Game could not be created");
                };
                roomCode = RandomString(6);
                try
                {
                    result = await _gameReopsitory.CreateGameAsync(roomCode, token);
                }
                catch (Exception) { } 
                tries++;
            }
            return roomCode;
        }

        public async Task<bool> JoinGameAsync(Player player, CancellationToken token = default)
        {
            await _playerValidator.ValidateAndThrowAsync(player, cancellationToken: token);
            
            var gameLeader = await _gameReopsitory.GetGameLeader(player.roomCode);

            if (gameLeader == null)
            {
                await _gameReopsitory.SetGameLeader(player, cancellationToken: token);
            }
            
            return await _gameReopsitory.JoinGameAsync(player, token);
        }

        public async Task<bool> EditGameAsync(GameSettings gameSettings, Guid userId, CancellationToken token = default)
        {
            await _gameSettingsValidator.ValidateAndThrowAsync(gameSettings, cancellationToken: token);

            if(await _gameReopsitory.GetGameLeader(gameSettings.roomCode, token) != userId)
            {
                throw new Exception("401;Unauthorized");
            }

            var result = await _gameReopsitory.EditGameAsync(gameSettings, cancellationToken: token);

            return result;
        }

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<bool> IsUserGameLeader(GameSettings gameSettings, Guid userId, CancellationToken token = default)
        {
            var gameLeader = await _gameReopsitory.GetGameLeader(gameSettings.roomCode, token);
            return gameSettings.gameLeader == gameLeader;
        }

        public async Task<bool> IsUserInGame(string roomCode, Guid userId)
        {
            var userGameId = await _gameReopsitory.GetUserGame(userId);
            return (userGameId is not null && userGameId != roomCode);
           
                
        }

        public async Task<int> NextRoundAsync(string roomCode, Guid userId, bool start, CancellationToken token = default)
        {
            // Validation
            if (!Regex.IsMatch(roomCode, @"(^[A-Za-z0-9]+$)") || roomCode.Length != 6)
            {
                throw new ValidationException("Invalid room code");
            }
            if (await _gameReopsitory.GetGameLeader(roomCode, token) != userId)
            {
                throw new Exception("401;Unauthorized");
            }
            if(await _gameReopsitory.GetCurrentRound(roomCode, token) > 0 && start)
            {
                throw new Exception("409;Game already started");
            }

            // Get word from wordlist
            string wlUrl = await _gameReopsitory.GetWordList(roomCode);
            HttpClient client = new HttpClient();
            string rawWordList = await client.GetStringAsync(wlUrl);
            var wordList = rawWordList.Split(Environment.NewLine);
            string word = wordList[random.Next(0, wordList.Length - 1)];

            return await _gameReopsitory.NextRoundAsync(roomCode, word, token);
        }

        public async Task<GameStatus> GetGameStatus(string roomCode, Guid userId, CancellationToken token = default)
        {
            var userGameCode = await _gameReopsitory.GetUserGame(userId);
            // Validation
            if (!Regex.IsMatch(roomCode, @"(^[A-Za-z0-9]+$)") || roomCode.Length != 6)
            {
                throw new ValidationException("Invalid room code");
            }
            if (userGameCode != roomCode)
            {
                throw new Exception("401;Unauthorized");
            }
            return await _gameReopsitory.GetGame(roomCode, token);
        }
    }
}
