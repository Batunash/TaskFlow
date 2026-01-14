using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class UpdateProjectDtoValidatorTest
    {
        private readonly UpdateProjectDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_Id_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new UpdateProjectDto { Id = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var model = new UpdateProjectDto { Name = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
        { 
            var model = new UpdateProjectDto { Name = new string('a', 101) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Maximum_Length()
        {
            var model = new UpdateProjectDto { Description = new string('a', 501) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new UpdateProjectDto
            {
                Id = 1,
                Name = "Valid Project Name",
                Description = "Valid Description"
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
