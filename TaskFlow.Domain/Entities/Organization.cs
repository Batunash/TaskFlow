using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;
namespace TaskFlow.Domain.Entities
{
    public class Organization : IAuditableEntity
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int OwnerId { get; private set; }

        private readonly List<OrganizationMember> _members = new();
        public IReadOnlyCollection<OrganizationMember> Members => _members;
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        private Organization() { }

        public Organization(string name,int ownerId)
        {
            if (string.IsNullOrWhiteSpace(name)) 
            { 
                throw new ArgumentException("Name required");
            }

            Name = name;
            OwnerId = ownerId;
            _members.Add(new OrganizationMember(ownerId, OrganizationRole.Owner));

        }

        public void AddMember(int userId, OrganizationRole role)
        {
            if (_members.Any(m => m.UserId == userId))
                return;

            _members.Add(new OrganizationMember(userId, role));
        }

        public bool IsAdmin(int userId)
        {
            return _members.Any(m =>
                m.UserId == userId &&
                m.Role == OrganizationRole.Admin
            );
        }
        public bool IsOwner(int userId)
        {
            return OwnerId == userId;
        }

    }

}
