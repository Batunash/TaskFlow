using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class AssignTaskDtoValidatorTest 
    {
        private readonly AssignTaskDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_TaskId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new AssignTaskDto { TaskId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.TaskId);
        }
        [Fact]
        public void Should_Have_Error_When_UserId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new AssignTaskDto { UserId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }
        [Fact]
        public void Should_Not_Have_Error_For_Valida_Model()
        {
            var model = new AssignTaskDto
            {
                TaskId = 1,
                UserId = 1
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
