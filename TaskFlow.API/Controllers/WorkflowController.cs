using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.DTOs;
namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api/projects/{projectId:int}/workflow")]
    [ApiController]
    public class WorkflowController(IWorkflowService workflowService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateWorkflow(int projectId)
        {
            try
            {
                var result = await workflowService.CreateWorkflowAsync(projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetWorkflow(int projectId)
        {
            try
            {
                var result = await workflowService.GetWorkflowAsync(projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("states")]
        public async Task<IActionResult> AddState(int projectId, WorkflowStateDto stateDto)
        {
            try
            {
                var result = await workflowService.AddStateAsync(projectId, stateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("transitions")]
        public async Task<IActionResult> AddTransition(int projectId, WorkflowTransitionDto transitionDto)
        {
            try
            {
                var result = await workflowService.AddTransitionAsync(projectId, transitionDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("states/{stateId:int}")]
        public async Task<IActionResult> RemoveState(int projectId, int stateId)
        {
            try
            {
                await workflowService.RemoveStateAsync(projectId, stateId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
        [HttpDelete("transitions/{transitionId:int}")]
        public async Task<IActionResult> RemoveTransition(int projectId, int transitionId)
        {
            try
            {
                await workflowService.RemoveTransitionAsync(projectId, transitionId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
