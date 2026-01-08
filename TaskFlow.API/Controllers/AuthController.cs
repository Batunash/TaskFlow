using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
namespace TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register(RegisterDto request)
        {
           var result = authService.Register(request);
            return Ok(result);
        }
        [HttpPost("login")]
        public IActionResult Login(LoginDto request)
        {
            var result = authService.Login(request);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst("userId")?.Value;
            var organizationId = User.FindFirst("organizationId")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userId == null || organizationId == null)
                return Unauthorized();

            return Ok(new
            {
                UserId = int.Parse(userId),
                OrganizationId = int.Parse(organizationId),
                Role = role
            });
        }
    }
}
