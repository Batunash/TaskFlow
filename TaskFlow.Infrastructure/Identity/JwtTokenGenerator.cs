using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;

namespace TaskFlow.Infrastructure.Identity
{
    public class JwtTokenGenerator(JwtSettings settings)
    {
        public string Generate(int userId, int organizationId,string role)
        {
            var claims = new[]
            {
                 new Claim("userId", userId.ToString()),
                 new Claim("organizationId", organizationId.ToString()),
                 new Claim(ClaimTypes.Role, role)
            };
            var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(settings.Secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
