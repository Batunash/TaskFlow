using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
    {
        public CreateProjectDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Proje adı boş olamaz.")
                .MaximumLength(100).WithMessage("Proje adı 100 karakteri geçemez.");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama 500 karakteri geçemez.");
        }
    }
}
