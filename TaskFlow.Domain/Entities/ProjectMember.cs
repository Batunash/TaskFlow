using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class ProjectMember : IHasOrganization
    {
        public int ProjectId { get; private set; }
        public Project? Project { get; private set; }

        public int UserId { get; private set; }
       
        public User? User { get; private set; }

        public Role Role { get; private set; }
        public int OrganizationId { get; set; }

        private ProjectMember() { }
        public ProjectMember(int projectId, int userId, Role role, int organizationId)
        {
            ProjectId = projectId;
            UserId = userId;
            Role = role;
            OrganizationId = organizationId;
        }
    }
}