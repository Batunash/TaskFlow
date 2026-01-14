using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.TestHelper;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Validators;
namespace TaskFlow.Tests.ApplicationTest.Validators
{
    public class TaskFilterDtoValidatorTest 
    {
        private readonly TaskFilterDtoValidator validator = new();
        [Fact]
        public void Should_Have_Error_When_PageNumber_Is_Less_Than_One()
        {
            var model = new TaskFilterDto { pageNumber = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.pageNumber);
        }

        [Fact]
        public void Should_Have_Error_When_PageSize_Is_Less_Than_One()
        {
            var model = new TaskFilterDto { pageSize = 0 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.pageSize);
        }

        [Fact]
        public void Should_Have_Error_When_PageSize_Is_Greater_Than_Fifty()
        {
            var model = new TaskFilterDto { pageSize = 51 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.pageSize);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new TaskFilterDto
            {
                pageNumber = 1,
                pageSize = 20 
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
