using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class AuthResponseDto
    {
        public int UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public int? OrganizationId { get; init; }
        public string AccessToken { get; init; } = string.Empty;

    }
}
