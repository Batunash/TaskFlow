using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class ChangeTaskStatusDtoValidator : AbstractValidator<ChangeTaskStatusDto>
    {
        public ChangeTaskStatusDtoValidator() 
        {
            RuleFor(x => x.TaskId).GreaterThan(0).WithMessage("Geçerli bir Görev ID girilmelidir.");
            RuleFor(x => x.TargetStateId).GreaterThan(0).WithMessage("Hedef Statü ID girilmelidir.");
        }
    }
}
