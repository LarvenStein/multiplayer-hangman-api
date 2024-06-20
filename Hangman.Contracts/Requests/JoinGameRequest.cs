using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Requests
{
    public class JoinGameRequest
    {
        public required String nickname { get; init; }
    }
}
