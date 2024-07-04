using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class JoinGameResponse
    {
        public required Guid Id {  get; init; }
        public required String nickname { get; init; }
        public required String roomCode { get; init; }

    }
}
