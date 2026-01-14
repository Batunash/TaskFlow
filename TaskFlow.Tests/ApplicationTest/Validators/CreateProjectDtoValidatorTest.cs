using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class CreateProjectDtoValidatorTest
    {
        private readonly CreateProjectDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var model = new CreateProjectDto { Name = "" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Proje adı boş olamaz.");
        }
        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            // Arrange
            var model = new CreateProjectDto { Name = new string('a', 101) };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Proje adı 100 karakteri geçemez.");
        }
        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            // Arrange
            var model = new CreateProjectDto { Description = new string('a', 501) };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Açıklama 500 karakteri geçemez.");
        }
        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            // Arrange
            var model = new CreateProjectDto
            {
                Name = "Geçerli Bir Proje",
                Description = "Açıklama"
            };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
