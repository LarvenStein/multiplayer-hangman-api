using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Models
{
    public partial class Player
    {
        public required Guid Id { get; init; }
        public required String Nickname { get; init; }
        public required String roomCode { get; init; }
    }
}
