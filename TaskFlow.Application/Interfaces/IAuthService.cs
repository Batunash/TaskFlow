using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto request);
        Task<AuthResponseDto> LoginAsync(LoginDto request);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }
}
