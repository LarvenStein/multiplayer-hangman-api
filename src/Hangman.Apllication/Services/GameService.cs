using FluentValidation;
using Hangman.Application.Models;
using Hangman.Application.Repository;
using Hangman.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly IUserRepository _userRepository;
        private readonly IGameStateRepository _gameStateRepository;
        private readonly IRandomService _randomService;
        private readonly IValidator<Player> _playerValidator;
        private readonly IValidator<GameSettings> _gameSettingsValidator;

        public GameService(IGameReopsitory gameReopsitory, IUserRepository userRepository, IGameStateRepository gameStateRepository, IValidator<Player> playerValidator, IValidator<GameSettings> settingsValidator, IRandomService randomService)
        {
            _gameReopsitory = gameReopsitory;
            _userRepository = userRepository;
            _gameStateRepository = gameStateRepository;
            _randomService = randomService;
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
                    throw new Exception("500;Game could not be created");
                };
                roomCode = _randomService.RandomString(6);
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
            
            var gameLeader = await _userRepository.GetGameLeader(player.roomCode);

            if (gameLeader == null)
            {
                await _userRepository.SetGameLeader(player, cancellationToken: token);
            }

            var playerList = await _gameReopsitory.GetAllPlayers(player.roomCode, token);
            var gameSettings = await _gameStateRepository.GetGame(player.roomCode, token);

            if (playerList.Count() >= gameSettings.maxPlayers)
            {
                throw new Exception("403;Game already full :(");
            }


            return await _gameReopsitory.JoinGameAsync(player, token);
        }

        public async Task<bool> EditGameAsync(GameSettings gameSettings, Guid userId, CancellationToken token = default)
        {
            await _gameSettingsValidator.ValidateAndThrowAsync(gameSettings, cancellationToken: token);

            if(await _userRepository.GetGameLeader(gameSettings.roomCode, token) != userId)
            {
                throw new Exception("401;Unauthorized");
            }

            var result = await _gameReopsitory.EditGameAsync(gameSettings, cancellationToken: token);

            return result;
        }

        public async Task<IEnumerable<string>> GetAllPlayers(string roomCode, Guid userId, CancellationToken token = default)
        {
            var userGameCode = await _userRepository.GetUserGame(userId);
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

        public async Task<IEnumerable<Wordlist>> GetWordlists(CancellationToken token = default)
        {
            return await _gameReopsitory.GetWordlists(token);
        }

        public async Task<int> StartGameAsync(string roomCode, Guid userId, CancellationToken token = default)
        {
            // Validation
            if (!Regex.IsMatch(roomCode, @"(^[A-Za-z0-9]+$)") || roomCode.Length != 6)
            {
                throw new ValidationException("Invalid room code");
            }
            if (await _userRepository.GetGameLeader(roomCode, token) != userId)
            {
                throw new Exception("401;Unauthorized");
            }

            var gameState = await _gameStateRepository.GetGame(roomCode, token);
            bool newGamePossible = gameState.rounds < (gameState.round + 1);



            // Delete old rounds
            var deleted = _gameStateRepository.DeleteRounds(roomCode);
            


            // Get word from wordlist
            int wordList = await _gameStateRepository.GetWordList(roomCode);
            var words = await _gameReopsitory.GetRandomWord(wordList, gameState.rounds);

            int i = 1;
            foreach (var word in words)
            {
                await _gameReopsitory.CreateRound(roomCode, word, i, token);
                i++;
            }

            return await _gameStateRepository.NextRoundAsync(roomCode, token);
        }
    }
}
