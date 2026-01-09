using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs
{
    public class ResponseTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; }
    }
}
