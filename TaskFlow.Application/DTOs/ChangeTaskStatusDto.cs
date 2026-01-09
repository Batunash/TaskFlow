using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs
{
    public class ChangeTaskStatusDto
    {
        public int TaskId { get; set; }
        public TaskItemStatus Status { get; set; }
    }

}
