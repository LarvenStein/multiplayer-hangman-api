using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Models
{
    public class RoundStatus
    {
        public required string roomCode { get; init; }
        public required Guid userId { get; init; }
        public required int roundNum { get; init; }
        public string? word { get; set; }
        public string? status { get; set; }
        public int correctGuesses { get; set; }
        public int falseGuesses { get; set; }
        public int livesLeft { get; set; }
        public List<char> guessedWord { get; set; } = new List<char>();
        public IEnumerable<string>? wrongLetters { get; set; }
    }
}
