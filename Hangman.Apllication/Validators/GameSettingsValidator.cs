using FluentValidation;
using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Validators
{
    public partial class GameSettingsValidator : AbstractValidator<GameSettings>
    {
        public GameSettingsValidator()
        {
            RuleFor(x => x.roomCode)
                .NotEmpty()
                .Matches(@"(^[A-Za-z0-9]+$)").WithMessage("Provide a valid room code")
                .Length(6);

            RuleFor(x => x.rounds)
                .NotEmpty()
                .LessThanOrEqualTo(25)
                .GreaterThanOrEqualTo(1);
            
            RuleFor(x => x.maxPlayers)
                .NotEmpty()
                .LessThanOrEqualTo(25)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.wordList)
                .NotEmpty();
        }
    }
}
