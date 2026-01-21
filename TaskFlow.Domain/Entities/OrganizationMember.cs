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
        public bool IsAccepted { get; private set; }
        public OrganizationRole Role { get; private set; }

        private OrganizationMember() { }

        public OrganizationMember(int userId, OrganizationRole role, bool isAccepted = false)
        {
            UserId = userId;
            Role = role;
            IsAccepted = isAccepted;
        }
        public void AcceptInvitation()
        {
            IsAccepted = true;
        }
    }

}
