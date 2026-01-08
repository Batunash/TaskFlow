using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; init; }
        public string UserName { get; init; } = string.Empty;
        public int OrganizationId { get; init; }
    }
}
