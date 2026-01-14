using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class WorkflowStateDtoValidatorTest 
    {
        private readonly WorkflowStateDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var model = new WorkflowStateDto { Name = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
        {
            var model = new WorkflowStateDto { Name = new string('a', 51) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new WorkflowStateDto
            {
                Name = "Valid Status Name"
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
