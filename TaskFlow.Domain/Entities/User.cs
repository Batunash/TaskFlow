using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string UserName { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        public int OrganizationId { get; private set; }

        private User() { }

        public User(string username, string passwordHash, int organizationId)
        {
            UserName = username;
            PasswordHash = passwordHash;
            OrganizationId = organizationId;
        }

    }
}
