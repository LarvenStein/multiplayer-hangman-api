using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Hangman.Contracts.Requests
{
    public class EditGameRequest
    {
        public required int rounds { get; init; }
        public required int maxPlayers { get; init; }
        public required Guid newGameLeader {  get; init; }
        public required int wordList { get; init; }

    }
}
