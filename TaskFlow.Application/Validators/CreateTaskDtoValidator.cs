using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Geçerli bir Proje ID girilmelidir.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Görev başlığı boş olamaz.")
                .MaximumLength(200).WithMessage("Görev başlığı 200 karakteri geçemez.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Açıklama 1000 karakteri geçemez.");
        }
    }
}
