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
            try
            {
                var result = await projectService.CreateProjectAsync(request, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var result = await projectService.GetAllProjectsAsync(currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            try
            {
                var result = await projectService.GetProjectByIdAsync(id, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(UpdateProjectDto request)
        {
            try
            {
                var result = await projectService.UpdateProjectAsync(request, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await projectService.DeleteProjectAsync(id, currentUserService.UserId!.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("{projectId}/members")]
        public async Task<IActionResult> AddMember(AddProjectMemberDto request)
        {
            try
            {
                await projectService.AddMemberAsync(request, currentUserService.UserId!.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("{projectId}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(RemoveProjectMemberDto request)
        {
            try
            {
                await projectService.RemoveMemberAsync(request, currentUserService.UserId!.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
