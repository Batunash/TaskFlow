using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Identity
{
    public class BcryptPasswordHasher : IPasswordHash
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
