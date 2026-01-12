using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;


namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationService organizationService,ICurrentUserService currentUserService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationDto request)
        {
            try
            {
                var result = await organizationService.CreateAsync(request, currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentOrganization()
        {
            try
            {
                var result = await organizationService.GetCurrentAsync(currentUserService.UserId!.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("invite")]
        public async Task<IActionResult> InviteUser(InviteUserDto request)
        {
            try
            {
                await organizationService.InviteAsync(request, currentUserService.UserId!.Value);
                return Ok(new { message = "User invited successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
