using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var result = await authService.RegisterAsync(request);  
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await authService.LoginAsync(request);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var isAuth = User.Identity?.IsAuthenticated;

            if (isAuth != true)
                return Unauthorized("Token authenticate edilmedi");

            var userId = User.FindFirst("userId")?.Value;
            var organizationId = User.FindFirst("organizationId")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userId == null || organizationId == null)
                return Unauthorized("Claim bulunamadı");

            return Ok(new
            {
                UserId = userId,
                OrganizationId = organizationId,
                Role = role
            });
        }

    }
}