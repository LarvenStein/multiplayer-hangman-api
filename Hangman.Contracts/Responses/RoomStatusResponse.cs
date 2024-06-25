using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class RoomStatusResponse
    {
        public required string roomCode { get; init; }
        public required int maxPlayers { get; init; }
        public required int rounds { get; init; }
        public required String wordList { get; init; }
        public required String status { get; init; }
        public required int round { get; init; }
    }
}
