using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces
{
    public interface ITaskService
    {
        Task<ResponseTaskDto> CreateAsync(CreateTaskDto dto, int currentUserId);

        Task<ResponseTaskDto> UpdateAsync(UpdateTaskDto dto, int currentUserId);

        Task<ResponseTaskDto> AssignAsync(AssignTaskDto dto, int currentUserId);

        Task DeleteAsync(int taskId, int currentUserId);
        Task<PageResult<ResponseTaskDto>> GetByFilterAsync(TaskFilterDto filter, int currentUserId);
        Task<IReadOnlyList<ResponseTaskDto>> GetByProjectIdAsync(int projectId,int currentUserId);
        Task ChangeStatusAsync(ChangeTaskStatusDto dto, int currentUserId);


    }
}
