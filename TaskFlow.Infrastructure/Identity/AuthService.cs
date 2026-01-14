using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Repositories;
using FluentValidation;
namespace TaskFlow.Infrastructure.Identity
{

    public class AuthService(IPasswordHash passwordHash, JwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository,
                            IValidator<RegisterDto>registerValidator,IValidator<LoginDto> loginValidator) : IAuthService
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
        {
            await registerValidator.ValidateAndThrowAsync(request);
            var existingUser = await userRepository.GetByUserNameAsync(request.UserName);
            if (existingUser != null)
            {
                throw new BusinessRuleException($"Username '{request.UserName}' is already taken.");
            }
            var hash = passwordHash.Hash(request.Password);

            var newUser = new User(
                request.UserName,
                hash,
                null
            );
           await userRepository.AddAsync(newUser);
            var token = jwtTokenGenerator.Generate(
                newUser.Id,
                null,
                "User"
            );
           

            return new AuthResponseDto
            {
                UserId = newUser.Id,
                UserName = newUser.UserName,
                OrganizationId = null,
                AccessToken = token
            };
        }
       public async Task<AuthResponseDto> LoginAsync(LoginDto request) 
        { 
            await loginValidator.ValidateAndThrowAsync(request);

            var user = await userRepository.GetByUserNameAsync(request.UserName);

            if (user == null)
            {
                throw new BusinessRuleException("Invalid credentials");
            }
            if (!passwordHash.Verify(request.Password, user.PasswordHash))
            {
                throw new BusinessRuleException("Invalid credentials");
            }
            var token = jwtTokenGenerator.Generate(
                user.Id,
                user.OrganizationId, 
                "User" 
            );

            return new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                OrganizationId = user.OrganizationId,
                AccessToken = token
            };
        }
        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
            {
               throw new NotFoundException($"User with ID {userId} not found");
            }
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                OrganizationId = user.OrganizationId
            };
        }
    }
}
