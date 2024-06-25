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
                var randomService = new RandomService();
                roomCode = randomService.RandomString(6);
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

        public async Task<IEnumerable<string>> GetAllPlayers(string roomCode, Guid userId, CancellationToken token = default)
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

            return await _gameReopsitory.GetAllPlayers(roomCode, token);


        }
    }
}
