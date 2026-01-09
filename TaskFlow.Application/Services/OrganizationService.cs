using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Services
{
    public class OrganizationService(IOrganizationRepository organizationRepository, ICurrentTenantService currentTenantService,ICurrentUserService currentUserService) : IOrganizationService
    {
        public async Task<ResponseOrganizationDto> CreateAsync(CreateOrganizationDto request, int currentUserId)
        {
           var organization = new Organization(
                name: request.Name,
                ownerId: currentUserId
            );
            await organizationRepository.AddAsync(organization);
            return new ResponseOrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name
            };
               
        }

        public async Task<ResponseOrganizationDto> GetCurrentAsync(int currentUserId)
        {
            var organizationId = currentTenantService.OrganizationId
                 ?? throw new UnauthorizedAccessException("Organization context not found");

            var organization = await organizationRepository.GetByIdAsync(organizationId)
                 ?? throw new Exception("Organization not found");

            if (!organization.IsOwner(currentUserId) &&
                !organization.Members.Any(m => m.UserId == currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            return new ResponseOrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name
            };
        }


        public async Task InviteAsync(InviteUserDto dto, int currentUserId)
        {
            var organizationId = currentTenantService.OrganizationId
                ?? throw new UnauthorizedAccessException("Organization context not found");

            var organization = await organizationRepository.GetByIdAsync(organizationId)
                ?? throw new Exception("Organization not found");

            if (!organization.IsOwner(currentUserId))
                throw new UnauthorizedAccessException();

            organization.AddMember(dto.UserId,OrganizationRole.Member);

            await organizationRepository.SaveChangesAsync();
        }


    }
}
