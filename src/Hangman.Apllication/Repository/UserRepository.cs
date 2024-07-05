using Dapper;
using Hangman.Application.Database;
using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task DeletePlayer(Guid userId)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                DELETE FROM Player
                WHERE PlayerId = (@userId)
                """, new {userId}));

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
