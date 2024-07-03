using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Database
{
        public interface IDbConnectionFactory
        {
            Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default);
        }

        public class MysqlConnectionFactory : IDbConnectionFactory
        {
            private readonly string _connectionString;

            public MysqlConnectionFactory(string connectionString)
            {
                _connectionString = connectionString;
            }

            public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default)
            {
                var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
                await connection.OpenAsync(token);
                return connection;
            }
        }

}
