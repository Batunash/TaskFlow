using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace TaskFlow.Application.Events
{
    public class TaskStatusChangedEvent : INotification
    {
        public int TaskId { get; }
        public string OldStateName { get; }
        public string NewStateName { get; }
        public int UserId { get; }
        public DateTime Timestamp { get; }
        public TaskStatusChangedEvent(int taskId, string oldStateName, string newStateName, int userId)
        {
            TaskId = taskId;
            OldStateName = oldStateName;
            NewStateName = newStateName;
            UserId = userId;
            Timestamp = DateTime.UtcNow;
        }
    }
}
