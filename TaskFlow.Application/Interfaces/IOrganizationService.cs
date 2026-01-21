using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<ResponseOrganizationDto> CreateAsync(CreateOrganizationDto dto, int currentUserId);

        Task<ResponseOrganizationDto> GetCurrentAsync();
        Task InviteAsync(InviteUserDto dto, int currentUserId);
        Task AcceptInvitationAsync(int organizationId, int currentUserId);
        Task<List<OrganizationInvitationDto>> GetMyInvitationsAsync(int currentUserId);
    }

}