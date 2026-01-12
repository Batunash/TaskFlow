using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<ResponseOrganizationDto> CreateAsync(CreateOrganizationDto dto, int currentUserId);

        Task<ResponseOrganizationDto> GetCurrentAsync();

        Task InviteAsync(InviteUserDto dto, int currentUserId);
    }

}