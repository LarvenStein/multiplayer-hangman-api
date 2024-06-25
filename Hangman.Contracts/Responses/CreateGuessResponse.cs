using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class CreateGuessResponse
    {
        public required String guess { get; init; }
        public required bool correct { get; init; }
        public required int roundNum { get; init; }

    }
}
