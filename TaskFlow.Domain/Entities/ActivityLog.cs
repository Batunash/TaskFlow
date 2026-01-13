using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public class ActivityLog
    {
        public int Id { get; private set; }
        public int TaskId { get; private set; }
        public string OldState { get; private set; } = string.Empty;
        public string NewState { get; private set; } = string.Empty;
        public int UserId { get; private set; }
        public DateTime Timestamp { get; private set; }

        private ActivityLog() { }

        public ActivityLog(int taskId, string oldState, string newState, int userId)
        {
            TaskId = taskId;
            OldState = oldState;
            NewState = newState;
            UserId = userId;
            Timestamp = DateTime.UtcNow;
        }
    }
}
