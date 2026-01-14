using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class TaskFilterDtoValidator : AbstractValidator<TaskFilterDto>
    {
        public TaskFilterDtoValidator()
        {
            RuleFor(x => x.pageNumber)
                    .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1'den küçük olamaz.");

            RuleFor(x => x.pageSize)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(50).WithMessage("Bir seferde en fazla 50 kayıt çekebilirsiniz.");
        }
    }
}
