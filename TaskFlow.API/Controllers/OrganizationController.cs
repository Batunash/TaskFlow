using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Identity;


namespace TaskFlow.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("GeneralPolicy")]
    public class OrganizationController(IOrganizationService organizationService,ICurrentUserService currentUserService,JwtTokenGenerator jwtTokenGenerator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrganization(CreateOrganizationDto request)
        {
            var userId = currentUserService.UserId!.Value;
            var result = await organizationService.CreateAsync(request, userId);
            var newToken = jwtTokenGenerator.Generate(
                userId,
                result.Id,
                OrganizationRole.Owner.ToString()
            );
            return Ok(new
            {
                Organization = result,
                AccessToken = newToken,
                Message = "Organization created. Token updated."
            });
        }
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentOrganization()
        {
            var result = await organizationService.GetCurrentAsync();
            return Ok(result);
        }
        [HttpPost("invite")]
        public async Task<IActionResult> InviteUser(InviteUserDto request)
        {
            await organizationService.InviteAsync(request, currentUserService.UserId!.Value);
            return Ok(new { message = "User invited successfully" });
        }

    }
}
