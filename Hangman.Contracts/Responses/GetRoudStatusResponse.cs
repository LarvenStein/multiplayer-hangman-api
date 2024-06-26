using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class GetRoudStatusResponse
    {
        public required string roomCode { get; init; }
        public required int roundNum { get; init; }
        public required string status { get; init; }
        public required int correctGuesses { get; init; }
        public required int falseGuesses { get; init; }
        public required int lifesLeft { get; init; } // Covering up my brutal spelling mistake
        public required List<char> guessedWord { get; init; }
        public required IEnumerable<string> wrongLetters { get; init; }

    }
}
