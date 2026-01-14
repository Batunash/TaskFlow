using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class RemoveProjectMemberDtoValidatorTest
    {
        private readonly RemoveProjectMemberDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_ProjectId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new RemoveProjectMemberDto { ProjectId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ProjectId);
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Less_Than_Or_Equal_To_Zero()
        {
            var model = new RemoveProjectMemberDto { UserId = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new RemoveProjectMemberDto
            {
                ProjectId = 1,
                UserId = 5
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
