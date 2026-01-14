using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class CreateOrganizationDtoValidator : AbstractValidator<CreateOrganizationDto>
    {
        public CreateOrganizationDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Organization name is required.")
                .MaximumLength(100).WithMessage("Organization name must not exceed 100 characters.");
        }
    }
}
