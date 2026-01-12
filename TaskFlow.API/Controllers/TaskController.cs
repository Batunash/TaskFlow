using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class TaskController(ITaskService taskService, ICurrentUserService currentUserService) : ControllerBase
    {
        [HttpPost("task")]
        public async Task<IActionResult> CreateTask(CreateTaskDto request)
        {
            try
            {
                var result = await taskService.CreateAsync(request, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpPut("task")]
        public async Task<IActionResult> UpdateTask(UpdateTaskDto request)
        {
            try
            {
                var result = await taskService.UpdateAsync(request, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("project/{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectId(int projectId)
        {
            try
            {
                var result = await taskService.GetByProjectIdAsync(projectId, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("task/assign")]
        public async Task<IActionResult> AssignTask(AssignTaskDto request)
        {
            try
            {
                var result = await taskService.AssignAsync(request, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpDelete("task/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                await taskService.DeleteAsync(taskId, currentUserService.UserId!.Value);
                return Ok(new { message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("task/status")]
        public async Task<IActionResult> ChangeTaskStatus(ChangeTaskStatusDto request)
        {
            try
            {
                await taskService.ChangeStatusAsync(request, currentUserService.UserId!.Value);
                return Ok(new { message = "Task status changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}
        
