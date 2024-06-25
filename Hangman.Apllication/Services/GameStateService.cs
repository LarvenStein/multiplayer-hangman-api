using FluentValidation;
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

        public GameStateService(IGameReopsitory gameReopsitory, IUserRepository userRepository, IGameStateRepository gameStateRepository, IValidator<Guess> guessValidator)
        {
            _gameReopsitory = gameReopsitory;
            _userRepository = userRepository;
            _gameStateRepository = gameStateRepository;
            _guessValidator = guessValidator;
        }

        private static Random random = new Random();

        public async Task<int> NextRoundAsync(string roomCode, Guid userId, bool start, CancellationToken token = default)
        {
            // Validation
            if (!Regex.IsMatch(roomCode, @"(^[A-Za-z0-9]+$)") || roomCode.Length != 6)
            {
                throw new ValidationException("Invalid room code");
            }
            if (await _userRepository.GetGameLeader(roomCode, token) != userId && start)
            {
                throw new Exception("401;Unauthorized");
            }
            if (await _gameStateRepository.GetCurrentRound(roomCode, token) > 0 && start)
            {
                throw new Exception("409;Game already started");
            }
            var gameState = await GetGameStatus(roomCode, userId, token);
            if (gameState.rounds < (gameState.round + 1)) return gameState.round + 1;

            // Get word from wordlist
            string wlUrl = await _gameStateRepository.GetWordList(roomCode);
            HttpClient client = new HttpClient();
            string rawWordList = await client.GetStringAsync(wlUrl);
            var wordList = rawWordList.Split(Environment.NewLine);
            string word = wordList[random.Next(0, wordList.Length - 1)];

            return await _gameStateRepository.NextRoundAsync(roomCode, word, token);
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

            if (userGameCode != guess.roomCode) throw new Exception("401;Unauthorized");
            if (await _gameStateRepository.GuessExsists(guess, token)) throw new Exception("400;Guess was already made");
            if (await _gameStateRepository.GetRoundState(guess.roomCode, guess.roundNum) != "active") throw new Exception("400;Round not active");

            string word = await _gameStateRepository.GetWord(guess.roomCode, guess.roundNum, token);
            char[] letters = word.ToCharArray();

            // Was a letter or entire word guessed?
            if (guess.guess.Length > 1)
            {
                if(word == guess.guess) guess.correct = true;
                else guess.correct = false;
            } else
            {
                guess.correct = searchLetter(guess.guess.ToCharArray()[0], letters);
            }
            await _gameStateRepository.MakeGuess(guess, token);

            int lifes = GameConstants.maxGuesses;
            char[] uniqueLetters = letters.Distinct().ToArray();
            int guessesNeeded = uniqueLetters.Length;
            int falseGuesses = await _gameStateRepository.CountIncorrectGuesses(guess, token);
            int correctGuesses = await _gameStateRepository.CountCorrectGuesses(guess, token);

            string state = "active";
            if(falseGuesses >= lifes)
            {
                state = "lost";
            }
            if((guess.guess .Length > 1 && guess.correct) || (guessesNeeded - correctGuesses) < 1)
            {
                state = "won";
            }
            await _gameStateRepository.SetRoundState(guess.roomCode, guess.roundNum, state, token);

            if (state != "active") await NextRoundAsync(guess.roomCode, guess.playerId, false); // No canceltoken

            return guess;
        }

        public bool searchLetter(char guessLetter, char[] letters)
        {
            bool found = false;

            foreach (char letter in letters)
            {
                if(letter == guessLetter) found = true;
            }
            return found;
        }
    }
}
