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
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Geçerli bir Kullanıcı ID girilmelidir.");
        }
    }
}
