using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces; 
using TaskFlow.Domain.Entities;
using MediatR;

namespace TaskFlow.Application.Events
{
    public class LogActivityHandler(IActivityLogRepository activityLogRepository): INotificationHandler<TaskStatusChangedEvent>
    {
        public async Task Handle(TaskStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            var log = new ActivityLog(
                notification.TaskId,
                notification.OldStateName,
                notification.NewStateName,
                notification.UserId
            );

            await activityLogRepository.AddAsync(log);
        }
    }
}
