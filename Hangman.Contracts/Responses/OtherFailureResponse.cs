﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Contracts.Responses
{
    public class OtherFailureResponse
    {
        public required string message { get; init; }
    }
}
