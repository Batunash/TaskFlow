using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;

namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class CreateTaskDtoValidatorTest
    {
        private readonly CreateTaskDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_ProjectId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new CreateTaskDto { ProjectId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ProjectId);
        }
        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var model = new CreateTaskDto { Title = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }
        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_Max_Length()
        {
            var model = new CreateTaskDto { Title = new string('a', 201) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }
        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Max_Length()
        {
            var model = new CreateTaskDto { Description = new string('a', 1001) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new CreateTaskDto
            {
                ProjectId = 1,
                Title = "Valid Title",
                Description = "Valid Description"
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}