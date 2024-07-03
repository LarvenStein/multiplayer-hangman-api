using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Requests
{
    public class CreateGuessRequest
    {
        public required string guess { get; init; }
    }
}
