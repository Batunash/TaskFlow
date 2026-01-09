using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
namespace TaskFlow.Domain.Entities
{
    public class OrganizationMember
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public OrganizationRole Role { get; private set; }

        private OrganizationMember() { }

        internal OrganizationMember(int userId, OrganizationRole role)
        {
            UserId = userId;
            Role = role;
        }
    }

}
