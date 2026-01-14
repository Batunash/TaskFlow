using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api/projects/{projectId:int}/workflow")]
    [ApiController]
    [EnableRateLimiting("GeneralPolicy")]
    public class WorkflowController(IWorkflowService workflowService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateWorkflow(int projectId)
        {
            var result = await workflowService.CreateWorkflowAsync(projectId);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetWorkflow(int projectId)
        {
            var result = await workflowService.GetWorkflowAsync(projectId);
            return Ok(result);
        }
        [HttpPost("states")]
        public async Task<IActionResult> AddState(int projectId, WorkflowStateDto stateDto)
        {
            var result = await workflowService.AddStateAsync(projectId, stateDto);
            return Ok(result);
        }
        [HttpPost("transitions")]
        public async Task<IActionResult> AddTransition(int projectId, WorkflowTransitionDto transitionDto)
        {
            var result = await workflowService.AddTransitionAsync(projectId, transitionDto);
            return Ok(result);
        }
        [HttpDelete("states/{stateId:int}")]
        public async Task<IActionResult> RemoveState(int projectId, int stateId)
        {
            await workflowService.RemoveStateAsync(projectId, stateId);
            return NoContent();

        }
        [HttpDelete("transitions/{transitionId:int}")]
        public async Task<IActionResult> RemoveTransition(int projectId, int transitionId)
        {
            await workflowService.RemoveTransitionAsync(projectId, transitionId);
            return NoContent();
        }

    }
}
