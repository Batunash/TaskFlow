using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController(IProjectService projectService, ICurrentUserService currentUserService) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDto request)
        {
            var result = await projectService.CreateProjectAsync(request, currentUserService.UserId!.Value);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var result = await projectService.GetAllProjectsAsync(currentUserService.UserId!.Value);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var result = await projectService.GetProjectByIdAsync(id, currentUserService.UserId!.Value);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(UpdateProjectDto request)
        {
            var result = await projectService.UpdateProjectAsync(request, currentUserService.UserId!.Value);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await projectService.DeleteProjectAsync(id, currentUserService.UserId!.Value);
            return NoContent();
        }
        [HttpPost("{projectId}/members")]
        public async Task<IActionResult> AddMember(AddProjectMemberDto request)
        {
            await projectService.AddMemberAsync(request, currentUserService.UserId!.Value);
            return NoContent();
        }
        [HttpPost("{projectId}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(RemoveProjectMemberDto request)
        {
            await projectService.RemoveMemberAsync(request, currentUserService.UserId!.Value);
            return NoContent();
        }

    }
}
