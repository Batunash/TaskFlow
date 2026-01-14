using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class CreateOrganizationDtoValidatorTest
    {
        private readonly CreateOrganizationDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var model = new CreateOrganizationDto { Name = "" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Organization name is required.");
        }
        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            // Arrange
            var model = new CreateOrganizationDto { Name = new string('a', 101) };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Organization name must not exceed 100 characters.");
        }
        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            // Arrange
            var model = new CreateOrganizationDto
            {
                Name = "Valid Organization Name"
            };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
