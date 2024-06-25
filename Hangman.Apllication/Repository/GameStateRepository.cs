using Dapper;
using Hangman.Application.Database;
using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                """, new { gameCode }, cancellationToken: token));
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

        public async Task<int> NextRoundAsync(string gameCode, string word, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);
            int roundNum = 0;
            roundNum += await GetCurrentRound(gameCode, token);

            roundNum++;

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO round (Word, RoundNum, RoomCode)
                VALUES (@word, @roundNum, @gameCode)

                """, new { gameCode, word, roundNum }, cancellationToken: token));

            if (result < 0) { }

            return roundNum;
        }
    }
}
