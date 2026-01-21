using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class InviteUserDtoValidator : AbstractValidator<InviteUserDto>
    {
        public InviteUserDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters.");
        }
    }
}
