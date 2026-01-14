using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class UpdateTaskDtoValidatorTest
    {
        private readonly UpdateTaskDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_Id_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new UpdateTaskDto { Id = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var model = new UpdateTaskDto { Title = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_Maximum_Length()
        {
            var model = new UpdateTaskDto { Title = new string('a', 201) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Maximum_Length()
        {
            var model = new UpdateTaskDto { Description = new string('a', 1001) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new UpdateTaskDto
            {
                Id = 1,
                Title = "Valid Task Title",
                Description = "Valid Description"
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
