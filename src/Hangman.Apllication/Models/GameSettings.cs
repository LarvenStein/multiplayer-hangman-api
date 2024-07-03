using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Models
{
    public partial class GameSettings
    {
        public required int rounds { get; init; }
        public required int maxPlayers { get; init; }
        public required Guid gameLeader { get; init; }
        public required string roomCode { get; init; }
        public required int wordList { get; init; }
    }
}
