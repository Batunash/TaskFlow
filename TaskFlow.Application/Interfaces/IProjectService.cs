using System.Collections.Generic;
using System.Threading.Tasks; 
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ResponseProjectDto> CreateProjectAsync(CreateProjectDto dto, int currentUserId);
        Task<ResponseProjectDto> UpdateProjectAsync(UpdateProjectDto dto, int currentUserId);
        Task DeleteProjectAsync(int projectId, int currentUserId);
        Task<ResponseProjectDto> GetByIdAsync(int projectId, int currentUserId);
        Task<IEnumerable<ResponseProjectDto>> GetAllProjectsAsync(int currentUserId);
        Task AddMemberAsync(AddProjectMemberDto dto, int currentUserId);
        Task RemoveMemberAsync(RemoveProjectMemberDto dto, int currentUserId);

    }
}
