﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Hangman.Application.Database;
using Hangman.Application.Models;
using MySqlX.XDevAPI.Common;

namespace Hangman.Application.Repository
{
    public class GameRepository : IGameReopsitory
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public GameRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> CreateGameAsync(string gameCode, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO Game (RoomCode)
                VALUES (@gameCode)
                """, new { gameCode }, cancellationToken: token));

            return result > 0;
        }

        public async Task<bool> EditGameAsync(GameSettings gameSettings, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Game
                SET GameLeader = (@gameLeader), Rounds = (@rounds), MaxPlayers = (@maxPlayers), WordList = (@wordList)
                WHERE RoomCode = (@roomCode)
                """, gameSettings, cancellationToken: cancellationToken));

            return result > 0;
        }

        public async Task<IEnumerable<string>> GetAllPlayers(string roomCode, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT Name
                FROM Player
                WHERE RoomCode = (@roomCode)
                """, new { roomCode }, cancellationToken: token));

            return result;
        }

        public async Task<IEnumerable<string>> GetRandomWord(int wordList, int words)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.QueryAsync<string>(new CommandDefinition("""
                SELECT Word
                FROM Words
                WHERE WordlistId = (@wordList)
                ORDER BY RAND() LIMIT @words
                """, new { wordList, words }));

            return result;
        }

        public async Task<IEnumerable<Wordlist>> GetWordlists(CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QueryAsync<Wordlist>(new CommandDefinition("""
                SELECT WordlistId AS Id, Name
                FROM Wordlist
                """, cancellationToken: token));

            return result;
        }

        public async Task<bool> JoinGameAsync(Player player, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);


            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO Player
                VALUES (@Id, @Nickname, @roomCode)
                """, player, cancellationToken: token));

            return result > 0;
        }

        public async Task<bool> CreateRound(string gameCode, string word, int roundNum, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO Round (Word, RoundNum, RoomCode)
                VALUES (@word, @roundNum, @gameCode)
                """, new { gameCode, roundNum, word }, cancellationToken: token));


            return result < 0;
        }

        public async Task DeleteGame(string roomCode)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM Game
                WHERE RoomCode = (@roomCode);
                
                DELETE FROM Guess
                WHERE RoomCode = (@roomCode);

                DELETE FROM Round
                WHERE RoomCode = (@roomCode);
                """, new { roomCode }));
        }

        public async Task NewGameLeader(string roomCode)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Game
                SET GameLeader = (SELECT Playerid
                                   FROM Player
                                   WHERE RoomCode = (@roomCode)
                                   LIMIT 1)
                WHERE RoomCode = (@roomCode)
                """, new { roomCode }));
        }
    }
}
