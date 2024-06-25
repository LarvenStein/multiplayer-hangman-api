using FluentValidation;
using Hangman.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Validators
{
    public class GuessValidator : AbstractValidator<Guess>
    {
        public GuessValidator()
        {
            RuleFor(x => x.playerId)
                .NotEmpty();

            RuleFor(x => x.roomCode)
                .NotEmpty()
                .Length(6)
                .Matches(@"(^[A-Za-z0-9]+$)").WithMessage("Provide a valid room code");

            RuleFor(x => x.roundNum)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.guess)
                .NotEmpty();

        }
    }
}
