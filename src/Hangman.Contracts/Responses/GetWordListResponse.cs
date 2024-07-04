using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class GetWordListResponse
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
    }
}
