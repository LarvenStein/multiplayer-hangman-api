﻿using FluentValidation;
using Hangman.Application.Models;
using Hangman.Application.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace Hangman.Application.Services
{
    public class GameStateService : IGameStateService
    {
        private readonly IGameReopsitory _gameReopsitory;
        private readonly IValidator<GameSettings> _gameSettingsValidator;

        public GameStateService(IGameReopsitory gameReopsitory, IValidator<GameSettings> gameSettingsValidator)
        {
            _gameReopsitory = gameReopsitory;
            _gameSettingsValidator = gameSettingsValidator;
        }

        private static Random random = new Random();

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
            if (await _gameReopsitory.GetCurrentRound(roomCode, token) > 0 && start)
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
