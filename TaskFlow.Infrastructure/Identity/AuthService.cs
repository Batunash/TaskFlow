using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories;
namespace TaskFlow.Infrastructure.Identity
{

    public class AuthService(IPasswordHash passwordHash, JwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository) : IAuthService
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
        {
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

    
            var user = await userRepository.GetByUserNameAsync(request.UserName);

            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }
            if (!passwordHash.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid credentials");
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
                throw new Exception("User not found"); 
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
