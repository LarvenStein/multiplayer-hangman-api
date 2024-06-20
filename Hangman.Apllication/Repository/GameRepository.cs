using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Hangman.Application.Database;
using Hangman.Application.Models;

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

        public async Task<bool> JoinGameAsync(Player player, CancellationToken token = default)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(token);

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO Player
                VALUES (@Id, @Nickname, @roomCode)
                """, player, cancellationToken: token));

            return result > 0;
        }
    }
}
