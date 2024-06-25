using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Services
{
    internal interface IRandomService
    {
        string RandomString(int length);
    }
}
