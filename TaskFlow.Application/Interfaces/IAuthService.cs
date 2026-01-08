using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces
{
    public interface IAuthService
    {
        AuthResponseDto Register(RegisterDto request);
        AuthResponseDto Login(LoginDto request);
    }
}
