using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class WorkflowTransitionDtoValidator : AbstractValidator<WorkflowTransitionDto>
    {
        public WorkflowTransitionDtoValidator() 
        {
            RuleFor(x => x.FromStateId).GreaterThan(0);
            RuleFor(x => x.ToStateId).GreaterThan(0);
            RuleFor(x => x).Must(x => x.FromStateId != x.ToStateId).WithMessage("Başlangıç ve Bitiş statüleri aynı olamaz.");
        }
    }
}
