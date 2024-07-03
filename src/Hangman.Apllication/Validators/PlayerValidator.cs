using FluentValidation;
using Hangman.Application.Models;
using Hangman.Application.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Application.Validators
{
    public class PlayerValidator : AbstractValidator<Player>
    {
        public PlayerValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
            RuleFor(x => x.Nickname)
                .NotEmpty()
                .Matches(@"([A-Za-z0-9_-]+)").WithMessage("Name should only contain letters.")
                .Length(3, 25);
            RuleFor(x => x.roomCode)
                .NotEmpty()
                .Matches(@"(^[A-Za-z0-9]+$)").WithMessage("Provide a valid room code")
                .Length(6);
        }
    }
}
