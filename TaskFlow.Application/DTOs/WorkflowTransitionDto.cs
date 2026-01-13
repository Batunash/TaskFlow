using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class WorkflowTransitionDto
    {
        public int? Id { get; set; }
        public int FromStateId { get; set; }
        public int ToStateId { get; set; }
        public List<string> AllowedRoles { get; set; } = [];
    }
}
