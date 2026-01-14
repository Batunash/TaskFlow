using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class AssignTaskDtoValidator : AbstractValidator<AssignTaskDto>
    {
        public AssignTaskDtoValidator()
        {
            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("Geçerli bir Görev ID girilmelidir.");
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Geçerli bir Kullanıcı ID girilmelidir.");
        }
    }
}
