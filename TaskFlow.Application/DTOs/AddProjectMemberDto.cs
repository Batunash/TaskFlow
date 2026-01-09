using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs
{
    public class AddProjectMemberDto
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public Role Role { get; set; }
    }
}
