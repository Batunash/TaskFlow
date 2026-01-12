using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;
namespace TaskFlow.Application.DTOs
{
    public class TaskFilterDto
    {
        public int projectId { get; set; }
        public TaskItemStatus? Status { get; set; }
        public int? AssignedUserId { get; set; }
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 10;


    }
}
