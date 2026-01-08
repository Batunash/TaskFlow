using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class LoginDto
    {
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public int OrganizationId { get; init; }
    }
}
