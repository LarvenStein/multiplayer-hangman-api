using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class GetPlayersResponse
    {
        public required IEnumerable<string> players { get; init; }
        public required bool isPlayerGameLeader { get; init; }
    }
}
