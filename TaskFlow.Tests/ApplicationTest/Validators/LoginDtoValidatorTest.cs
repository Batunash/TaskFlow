using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;

namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class LoginDtoValidatorTest
    {
        private readonly LoginDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_UserName_Empty()
        {
            var model = new LoginDto { UserName = "", Password = "1234" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserName);

        }
        [Fact]
        public void Should_Have_Error_When_Password_Empty()
        {
            var model = new LoginDto { UserName = "test", Password = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);

        }
        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new LoginDto { UserName = "test", Password = "1234" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();

        }
    }
}
