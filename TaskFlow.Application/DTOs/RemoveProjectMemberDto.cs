using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class RemoveProjectMemberDto
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }

    }
}
