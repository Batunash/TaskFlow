using FluentValidation;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class ChangeTaskStatusDtoValidatorTest 
    {
        private readonly ChangeTaskStatusDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_TaskId_Is_Less_Than_Or_Equal_To_Zero()
        {
            // Arrange
            var model = new ChangeTaskStatusDto { TaskId = 0 };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaskId);
        }

        [Fact]
        public void Should_Have_Error_When_TargetStateId_Is_Less_Than_Or_Equal_To_Zero()
        {
            // Arrange
            var model = new ChangeTaskStatusDto { TargetStateId = 0 };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TargetStateId);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            // Arrange
            var model = new ChangeTaskStatusDto
            {
                TaskId = 1,
                TargetStateId = 2 
            };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
