using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class RegisterDtoValidatorTest 
    {
        private readonly RegisterDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_UserName_Is_Empty()
        {
            var model = new RegisterDto { UserName = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Too_Short()
        {
            
            var model = new RegisterDto { UserName = "ab" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Too_Long()
        { 
            var model = new RegisterDto { UserName = new string('a', 51) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }


        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var model = new RegisterDto { Password = string.Empty };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            var model = new RegisterDto { Password = "123" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Contain_Uppercase()

        {
            var model = new RegisterDto { Password = "password123" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Contain_Lowercase()
        {
            var model = new RegisterDto { Password = "PASSWORD123" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Does_Not_Contain_Digit()
        {
            var model = new RegisterDto { Password = "Password" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new RegisterDto
            {
                UserName = "ValidUser",
                Password = "Password123" 
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
