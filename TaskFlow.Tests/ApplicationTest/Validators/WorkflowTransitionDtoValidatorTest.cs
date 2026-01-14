using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class WorkflowTransitionDtoValidatorTest 
    {
        private readonly WorkflowTransitionDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_FromStateId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new WorkflowTransitionDto { FromStateId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.FromStateId);
        }

        [Fact]
        public void Should_Have_Error_When_ToStateId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new WorkflowTransitionDto { ToStateId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ToStateId);
        }

        [Fact]
        public void Should_Have_Error_When_FromStateId_And_ToStateId_Are_Same()
        {
            var model = new WorkflowTransitionDto
            {
                FromStateId = 1,
                ToStateId = 1
            };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("Başlangıç ve Bitiş statüleri aynı olamaz.");
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new WorkflowTransitionDto
            {
                FromStateId = 1,
                ToStateId = 2 
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
