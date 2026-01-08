using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.DTOs;
using Microsoft.Identity.Client;
using TaskFlow.Infrastructure.Repositories;
namespace TaskFlow.Infrastructure.Identity
{

    public class AuthService(IPasswordHash passwordHash,JwtTokenGenerator jwtTokenGenerator,UserRepository user) : IAuthService
    {
        public AuthResponseDto Register(RegisterDto request)
        {
            var hash = passwordHash.Hash(request.Password);

            var user = new user(
                request.UserName,
                hash,
                request.OrganizationId
            );
            var token = jwtTokenGenerator.Generate(
            user.Id,
            user.OrganizationId,
            "User"
            );

            return new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.Name,
                OrganizationId = user.OrganizationId,
                AccessToken = token
            };
        }
        public AuthResponseDto Login(LoginDto request)
        {
            var user = user.GetByUserName(
               request.UserName,
               request.OrganizationId);

            if (user == null)
                throw new Exception("Invalid credentials");

            if (!passwordHash.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var token = jwtTokenGenerator.Generate(
                user.Id,
                user.OrganizationId,
                "User" 
            );
            return new AuthResponseDto
            {
                UserId = user.Id,
                UserName = user.Name,
                OrganizationId = user.OrganizationId,
                AccessToken = token
            };

        }
    }
