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
            var result = await taskService.CreateAsync(request, currentUserService.UserId!.Value);
            return Ok(result);

        }
        [HttpPut("task")]
        public async Task<IActionResult> UpdateTask(UpdateTaskDto request)
        {
            var result = await taskService.UpdateAsync(request, currentUserService.UserId!.Value);
            return Ok(result);
        }
        [HttpGet("project/{projectId}/tasks")]
        public async Task<IActionResult> GetTasksByProjectId(int projectId)
        {
            var result = await taskService.GetByProjectIdAsync(projectId, currentUserService.UserId!.Value);
            return Ok(result);
        }
        [HttpGet("task")]
        public async Task<IActionResult> GetTasks([FromQuery] TaskFilterDto filter)
        {
            var userId = currentUserService.UserId!.Value;
            var result = await taskService.GetByFilterAsync(filter, userId);
            return Ok(result);
        }
        [HttpPost("task/assign")]
        public async Task<IActionResult> AssignTask(AssignTaskDto request)
        {
            var result = await taskService.AssignAsync(request, currentUserService.UserId!.Value);
            return Ok(result);

        }
        [HttpDelete("task/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            await taskService.DeleteAsync(taskId, currentUserService.UserId!.Value);
            return Ok(new { message = "Task deleted successfully" });
        }
        [HttpPost("task/status")]
        public async Task<IActionResult> ChangeTaskStatus(ChangeTaskStatusDto request)
        {
            await taskService.ChangeStatusAsync(request, currentUserService.UserId!.Value);
            return Ok(new { message = "Task state changed successfully" });
        }
    }
}
        
