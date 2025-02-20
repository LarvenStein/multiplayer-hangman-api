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
        private readonly IUserRepository _userRepository;
        private readonly IGameStateRepository _gameStateRepository;
        private readonly IValidator<Guess> _guessValidator;
        private readonly IValidator<RoundStatus> _roundStatusValidator;

        public GameStateService(IGameReopsitory gameReopsitory, IUserRepository userRepository, IGameStateRepository gameStateRepository, IValidator<Guess> guessValidator, IValidator<RoundStatus> roundStatusValidator)
        {
            _gameReopsitory = gameReopsitory;
            _userRepository = userRepository;
            _gameStateRepository = gameStateRepository;
            _guessValidator = guessValidator;
            _roundStatusValidator = roundStatusValidator;
        }

        private static Random random = new Random();
        private static char[] acceptableLetters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public async Task<int> NextRoundAsync(string roomCode, Guid userId, bool start, CancellationToken token = default)
        {
            // Validation
            if (!Regex.IsMatch(roomCode, @"(^[A-Za-z0-9]+$)") || roomCode.Length != 6)
            {
                throw new ValidationException("Invalid room code");
            }
            /*if (await _userRepository.GetGameLeader(roomCode, token) != userId)
            {
                throw new Exception("401;Unauthorized");
            }*/

            var gameState = await _gameStateRepository.GetGame(roomCode, token);
            bool newGamePossible = gameState.rounds < (gameState.round + 1);



            // If startGame requested, delete old rounds
            if (start)
            {
                var deleted = _gameStateRepository.DeleteRounds(roomCode);
            }

            if (newGamePossible && !start) return gameState.round + 1;


/*            // Get word from wordlist
            int wordList = await _gameStateRepository.GetWordList(roomCode);
            string word = await _gameReopsitory.GetRandomWord(wordList);*/

            return await _gameStateRepository.NextRoundAsync(roomCode, token);
        }

        public async Task<GameStatus> GetGameStatus(string roomCode, Guid userId, CancellationToken token = default)
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
            return await _gameStateRepository.GetGame(roomCode, token);
        }

        public async Task<Guess> HandleGuess(Guess guess, CancellationToken token = default)
        {
            await _guessValidator.ValidateAndThrowAsync(guess, cancellationToken: token);
            var userGameCode = await _userRepository.GetUserGame(guess.playerId);

            if (await _gameStateRepository.GetRoundState(guess.roomCode, guess.roundNum) != "active") throw new Exception("400;Round not active");
            if (userGameCode != guess.roomCode) throw new Exception("401;Unauthorized");
            if (await _gameStateRepository.GuessExsists(guess, token)) throw new Exception("400;Guess was already made");

            string word = await _gameStateRepository.GetWord(guess.roomCode, guess.roundNum, token);
            char[] letters = word.ToCharArray();

            // Was a letter or entire word guessed?
            if (guess.guess.Length > 1)
            {
                if(word == guess.guess) guess.correct = true;
                else guess.correct = false;
            } else
            {
                bool found = false;

                foreach (char letter in letters)
                {
                    if (letter.ToString() == guess.guess) found = true;
                }

                guess.correct = found;
            }
            await _gameStateRepository.MakeGuess(guess, token);

            int lifes = GameConstants.maxGuesses;
            char[] uniqueAcceptableLetters = letters.Distinct().Where(letter => acceptableLetters.Contains(Char.ToUpper(letter))).ToArray();
            int guessesNeeded = uniqueAcceptableLetters.Length;
            int falseGuesses = await _gameStateRepository.CountIncorrectGuesses(guess, token);
            int correctGuesses = await _gameStateRepository.CountCorrectGuesses(guess, token);

            string state = "active";
            if(falseGuesses >= lifes)
            {
                state = "lost";
            }
            if((guess.guess.Length > 1 && guess.correct) || (guessesNeeded - correctGuesses) < 1)
            {
                state = "won";
            }
            await _gameStateRepository.SetRoundState(guess.roomCode, guess.roundNum, state, token);

            if (state != "active") await NextRoundAsync(guess.roomCode, guess.playerId, false); // No canceltoken

            return guess;
        }

        public async Task<RoundStatus> GetRoundStatus(RoundStatus status, CancellationToken token = default)
        {
            var userGameCode = await _userRepository.GetUserGame(status.userId);
            await _roundStatusValidator.ValidateAndThrowAsync(status, token);
            if (userGameCode != status.roomCode) throw new Exception("401;Unauthorized");
            
            status.status = await _gameStateRepository.GetRoundState(status.roomCode, status.roundNum, token);
            if(status.status == "inactive")
            {
                throw new Exception("404;Round not found");
            }
            status.word = await _gameStateRepository.GetWord(status.roomCode, status.roundNum, token);


            // I played myself a bit by making the argument for the counting methods a 'Guess'
            // But no problem for me :>
            var fakeGuess = new Guess { roomCode = status.roomCode, roundNum = status.roundNum, playerId = status.userId, guess = "" };

            status.falseGuesses = await _gameStateRepository.CountIncorrectGuesses(fakeGuess);
            status.correctGuesses = await _gameStateRepository.CountCorrectGuesses(fakeGuess);
            status.livesLeft = GameConstants.maxGuesses - status.falseGuesses;
            status.wrongLetters = await _gameStateRepository.GetWrongGuesses(status.roomCode, status.roundNum, token);

            fakeGuess = null;
            char[] wordArray = status.word.ToCharArray();
            foreach (char letter in wordArray)
            {
                fakeGuess = new Guess { roomCode = status.roomCode, roundNum = status.roundNum, playerId = status.userId, guess = letter.ToString() };
                bool guessExsists = await _gameStateRepository.GuessExsists(fakeGuess);
                if (guessExsists || status.status != "active" || !acceptableLetters.Contains(Char.ToUpper(letter)))
                {
                    status.guessedWord.Add(letter);
                } else
                {
                    status.guessedWord.Add('_');
                }
                fakeGuess = null;
            }
            return status;
        }

        public async Task<IEnumerable<RoundStatus>> GetRoundsStatus(string roomCode, Guid userId, CancellationToken token = default)
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
            int currentRound = await _gameStateRepository.GetCurrentRound(roomCode,token);

            List<RoundStatus> result = new List<RoundStatus>();

            for (int i = 1; i <= currentRound; i++)
            {
                var status = new RoundStatus { roomCode= roomCode, roundNum= i, userId = userId };
                var roundStatus = await GetRoundStatus(status, token);
                result.Add(roundStatus);
            }

            return result;
        }
    }
}
