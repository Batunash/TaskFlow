using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Identity;


namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationService organizationService,ICurrentUserService currentUserService,JwtTokenGenerator jwtTokenGenerator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationDto request)
        {
            try
            {
                var userId = currentUserService.UserId!.Value;
                var result = await organizationService.CreateAsync(request, userId);
                var newToken = jwtTokenGenerator.Generate(
                    userId,
                    result.Id, // Yeni Org ID
                    OrganizationRole.Owner.ToString()
                );
                return Ok(new
                {
                    Organization = result,
                    AccessToken = newToken,
                    Message = "Organization created. Token updated."
                });
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
                var result = await organizationService.GetCurrentAsync();
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
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
