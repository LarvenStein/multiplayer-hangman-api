using System;
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

        public async Task<int> GetCurrentRound(string gameCode, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            int roundNum = 0;
            roundNum += await connection.QuerySingleAsync<int>(new CommandDefinition("""
                SElECT COUNT(*)
                FROM round
                WHERE RoomCode = (@gameCode)
                """, new { gameCode }, cancellationToken: cancellationToken));

            return roundNum;

        }

        public async Task<GameStatus> GetGame(string gameCode, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.QuerySingleAsync<GameStatus>(new CommandDefinition("""
                SELECT game.RoomCode, 
                        game.MaxPlayers, 
                        game.Rounds, 
                        wordlist.name AS Wordlist, 
                        (SELECT CASE 
                            WHEN COUNT(*) = 0 THEN 'Lobby'
                            -- TODO: Put something here for maxrounds + 1
                            ELSE 'playing'
                        END AS Status
                        FROM Round
                        WHERE RoomCode = (@gameCode)) AS status,
                        (SElECT COUNT(*)
                        FROM round
                        WHERE RoomCode = (@gameCode)) AS round
                FROM game INNER JOIN wordlist
                ON game.WordList = wordlist.WordlistId
                WHERE game.roomCode = (@gameCode)
                """, new {gameCode}, cancellationToken: token));
            return result;
        }

        public async Task<Guid?> GetGameLeader(string gameCode, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            var result = await connection.QuerySingleAsync<Guid?>(new CommandDefinition("""
                SELECT GameLeader
                FROM Game
                WHERE RoomCode = @gameCode
                """, new { gameCode }, cancellationToken: cancellationToken));

            return result;
        }

        public async Task<string?> GetUserGame(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.QuerySingleAsync<string?>(new CommandDefinition("""
                SELECT RoomCode
                FROM Player
                WHERE PlayerId = @userId
                """, new { userId }));
            return result;
        }

        public async Task<string> GetWordList(string gameCode, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.QuerySingleAsync<string>(new CommandDefinition("""
                SELECT wordlist.Path
                FROM game INNER JOIN wordlist
                ON game.WordList = wordlist.WordlistId
                WHERE game.Roomcode = (@gameCode)
                """, new { gameCode }, cancellationToken: cancellationToken));
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

        public async Task<int> NextRoundAsync(string gameCode, string word, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            int roundNum = 0;
            roundNum += await GetCurrentRound(gameCode, token);

            roundNum++;

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO round (Word, RoundNum, RoomCode)
                VALUES (@word, @roundNum, @gameCode)

                """, new {gameCode, word, roundNum}, cancellationToken: token));
            
            if (result < 0) { }

            return roundNum;
        }

        public async Task<bool> SetGameLeader(Player player, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Game
                SET GameLeader = (@Id)
                WHERE RoomCode = (@roomCode)
                """, player, cancellationToken: token));

            return result > 0;
        }
    }
}
