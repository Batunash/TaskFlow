using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
using Xunit;

namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class InviteUserDtoValidatorTest
    {
        private readonly InviteUserDtoValidator validator = new();

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Empty()
        {
            var model = new InviteUserDto { UserName = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new InviteUserDto { UserName = "validUser" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}