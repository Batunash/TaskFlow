using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.API.Controllers
{
    public class ProjectController : Controller
    {

        [Route("api/[controller]")]
        [ApiController]
        public class ProjectController(IProjectService projectService) : ControllerBase
        {
            [HttpGet("{id}")]
            public async Task<IActionResult> GetProjectById(int id)
            {
                var result = await projectService.GetProjectByIdAsync(id);
                return Ok(result);
            }
            [HttpPost]
            public async Task<IActionResult> CreateProject(CreateProjectDto request)
            {
                var result = await projectService.CreateProjectAsync(request);
                return Ok(result);
            }
       
    }
}
