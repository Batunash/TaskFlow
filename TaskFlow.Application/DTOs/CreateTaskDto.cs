using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;
namespace TaskFlow.Application.DTOs
{
    public class CreateTaskDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

}

