using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Validators
{
    public class WorkflowStateDtoValidator : AbstractValidator<WorkflowStateDto>
    {
        public WorkflowStateDtoValidator() 
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Statü adı boş olamaz.").MaximumLength(50).WithMessage("Statü adı 50 karakteri geçemez.");
        }
    }
}
