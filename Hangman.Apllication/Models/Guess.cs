using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Models
{
    public partial class Guess
    {
        public required Guid playerId { get; init; }
        public required string roomCode { get; init; }
        public required int roundNum { get; init; }
        public required string guess { get; init; }
        public bool correct { get; set; }
    }
}
