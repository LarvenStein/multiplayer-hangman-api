using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Database
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            // Creating the game table
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Game (
                RoomCode CHAR(6) primary key,
                MaxPlayers TINYINT not null DEFAULT 5,
                Rounds TINYINT not null DEFAULT 5,
                WordList INT not null DEFAULT 1,
                GameLeader CHAR(36))
                """);

            // Creating the player table
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Player (
                PlayerId CHAR(36) primary key,
                Name TINYTEXT not null,
                RoomCode CHAR(6) not null
                )
                """);

            // Creating the round table
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Round (
                Word TINYTEXT not null,
                RoundNum TINYINT not null,
                Status TINYTEXT not null DEFAULT "active",
                RoomCode CHAR(6) not null,
                PRIMARY KEY (RoundNum, RoomCode)
                )
                """);

            // Creating the guesses table
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Guess (
                -- Gu
                PlayerId CHAR(36),
                RoomCode CHAR(6),
                RoundNum TINYINT,
                Guess TEXT not null,
                Correct BOOL not null
                )
                """);

            // Creating wordlists table
            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Wordlist (
                WordlistId INT primary key AUTO_INCREMENT,
                Path TEXT,
                Name TEXT)
                """);
        }
    }
}
