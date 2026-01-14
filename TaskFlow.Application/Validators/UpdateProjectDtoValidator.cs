using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
    {
        public UpdateProjectDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçerli bir Proje ID girilmelidir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Proje adı boş olamaz.")
                .MaximumLength(100).WithMessage("Proje adı 100 karakteri geçemez.");

            RuleFor(x => x.Description)
               .MaximumLength(500).WithMessage("Proje açıklaması 500 karakteri geçemez.");
        }
    }

}
