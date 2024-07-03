using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Models
{
    public partial class Wordlist
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
    }
}
