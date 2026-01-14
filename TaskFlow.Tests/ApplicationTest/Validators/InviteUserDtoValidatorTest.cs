using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class InviteUserDtoValidatorTest
    {
        private readonly InviteUserDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_TaskId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new InviteUserDto { UserId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }
        [Fact]
        public void Shoul_Not_Have_For_Valid_Model()
        {
            var mode = new InviteUserDto { UserId = 1 };
            var result = validator.TestValidate(mode);
            result.ShouldNotHaveAnyValidationErrors();

        }
    }
}
