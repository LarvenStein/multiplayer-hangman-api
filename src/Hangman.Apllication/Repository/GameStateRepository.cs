﻿using Dapper;
using Hangman.Application.Database;
using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangman.Application.Repository
{
    public class GameStateRepository : IGameStateRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public GameStateRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> CountCorrectGuesses(Guess guess, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            int roundNum = 0;
            roundNum += await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SElECT COUNT(*)
                FROM Guess
                WHERE RoomCode = (@roomCode)
                AND RoundNum = (@roundNum)
                AND Correct = 1
                """, guess, cancellationToken: token));

            return roundNum;
        }

        public async Task<int> CountIncorrectGuesses(Guess guess, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            int roundNum = 0;
            roundNum += await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SElECT COUNT(*)
                FROM Guess
                WHERE RoomCode = (@roomCode)
                AND RoundNum = (@roundNum)
                AND Correct = 0
                """, guess, cancellationToken: token));

            return roundNum;
        }

        public async Task<bool> DeleteRounds(string roomCode)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM Round
                WHERE RoomCode = (@roomCode);
                DELETE FROM Guess
                WHERE RoomCode = (@roomCode);
                """, new { roomCode }));
            return result > 0;
        }

        public async Task<int> GetCurrentRound(string gameCode, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            int roundNum = 0;
            roundNum += await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SElECT COUNT(*)
                FROM Round
                WHERE NOT Status = "inactive"
                AND RoomCode = (@gameCode)
                """, new { gameCode }, cancellationToken: cancellationToken));

            return roundNum;

        }

        public async Task<GameStatus> GetGame(string gameCode, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<GameStatus>(new CommandDefinition("""
                SELECT Game.RoomCode, 
                        Game.MaxPlayers, 
                        Game.Rounds, 
                        Wordlist.name AS Wordlist, 
                        (SELECT CASE 
                            WHEN COUNT(*) = 0 THEN 'lobby'
                            WHEN (SELECT COUNT(*)
                                  FROM Round 
                                  WHERE RoomCode = (@gameCode)
                                  AND status = "active") = 0 THEN 'done'
                            ELSE 'playing'
                        END AS Status
                        FROM Round
                        WHERE RoomCode = (@gameCode)) AS status,
                        (SELECT COUNT(*)
                        FROM Round
                        WHERE NOT Status = "inactive"
                        AND RoomCode = (@gameCode)) AS round
                FROM Game INNER JOIN Wordlist
                ON Game.WordList = Wordlist.WordlistId
                WHERE Game.roomCode = (@gameCode)
                """, new { gameCode }, cancellationToken: token));
            return result;
        }

        public async Task<string> GetRoundState(string gameCode, int round, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<string>(new CommandDefinition("""
                SELECT Status   
                FROM Round
                WHERE RoomCode = (@gameCode)
                AND RoundNum = (@round)
                """, new { gameCode, round }, cancellationToken: token));
            return result;
        }

        public async Task<string> GetWord(string gameCode, int round, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<string>(new CommandDefinition("""
                SELECT Word
                FROM Round
                WHERE RoomCode = (@gameCode)
                AND RoundNum = (@round)
                """, new { gameCode, round }, cancellationToken: token));
            return result.ToUpper();
        }

        public async Task<int> GetWordList(string gameCode, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SELECT Wordlist.WordlistId
                FROM Game INNER JOIN Wordlist
                ON Game.WordList = Wordlist.WordlistId
                WHERE Game.Roomcode = (@gameCode)
                """, new { gameCode }, cancellationToken: cancellationToken));
            return result;
        }

        public async Task<IEnumerable<string>> GetWrongGuesses(string roomCode, int roundNum, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            return await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT Guess
                FROM Guess
                WHERE RoomCode = (@roomCode)
                AND RoundNum = (@roundNum)
                AND Correct = 0
                """, new { roomCode, roundNum }, cancellationToken: token));
        }

        public async Task<bool> GuessExsists(Guess guess, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SELECT COUNT(*)
                FROM Guess
                WHERE RoomCode = (@roomCode)
                AND RoundNum = (@roundNum)
                AND Guess = (@guess)
                """, guess, cancellationToken: token));
            return result >= 1;
        }

        public async Task<bool> MakeGuess(Guess guess, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO Guess
                VALUES (@playerId, @roomCode, @roundNum, @guess, @correct)

                """, guess, cancellationToken: token));

            return result > 0;
        }

        public async Task<int> NextRoundAsync(string gameCode, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            int roundNum = 0;
            roundNum += await GetCurrentRound(gameCode, token);

            roundNum++;

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Round
                SET Status = "active"
                WHERE RoundNum = (@roundNum)
                AND RoomCode = (@gameCode)

                """, new { gameCode, roundNum }, cancellationToken: token));

            if (result < 0) { }


            return roundNum;
        }

        public async Task<bool> SetRoundState(String roomCode, int roundNum, string newState, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Round
                SET Status = (@newState)
                WHERE RoomCode = (@roomCode)
                AND RoundNum = (@roundNum)
                """, new { roomCode, roundNum, newState}, cancellationToken: token));

            return result > 0;
        }
    }
}
