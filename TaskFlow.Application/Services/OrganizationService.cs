using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Services
{
    public class OrganizationService(IOrganizationRepository organizationRepository, ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService,IUserRepository userRepository,
        IValidator<CreateOrganizationDto> createOrgValidator,IValidator<InviteUserDto> inviteUserValidator
        ) : IOrganizationService
    {
        public async Task<ResponseOrganizationDto> CreateAsync(CreateOrganizationDto request, int currentUserId)
        {
            var exists = await organizationRepository.ExistsByNameAsync(request.Name);
            if (exists)
            {
                throw new BusinessRuleException($"Organization with name '{request.Name}' already exists.");
            }
            await createOrgValidator.ValidateAndThrowAsync(request);

            var organization = new Organization(
                name: request.Name,
                ownerId: currentUserId
            );

            await organizationRepository.AddAsync(organization);
            var user = await userRepository.GetByIdAsync(currentUserId);
            if (user != null)
            {
                user.OrganizationId = organization.Id; 
                await organizationRepository.SaveChangesAsync();
            }

            return new ResponseOrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name
            };
        }

        public async Task<ResponseOrganizationDto> GetCurrentAsync() 
        {
            var currentUserId = currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User context not found");
            var organizationId = currentTenantService.OrganizationId
                    ?? throw new UnauthorizedAccessException("Organization context not found");
            var organization = await organizationRepository.GetByIdWithMembersAsync(organizationId)
                    ?? throw new NotFoundException($"Organization with ID {organizationId} not found.");
            if (!organization.IsOwner(currentUserId) &&!organization.Members.Any(m => m.UserId == currentUserId))
            {
                throw new UnauthorizedAccessException("User is not a member of this organization.");
            }

            return new ResponseOrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name
            };
        }


        public async Task InviteAsync(InviteUserDto dto, int currentUserId)
        {
            await inviteUserValidator.ValidateAndThrowAsync(dto);
            var organizationId = currentTenantService.OrganizationId
                ?? throw new UnauthorizedAccessException("Organization context not found");

            var organization = await organizationRepository.GetByIdAsync(organizationId)
                ?? throw new NotFoundException($"Organization with ID {organizationId} not found.");

            if (!organization.IsOwner(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            organization.AddMember(dto.UserId,OrganizationRole.Member);
            var user = await userRepository.GetByIdAsync(dto.UserId);
            if (user != null && user.OrganizationId == null)
            {
                user.OrganizationId = organization.Id;
            }
            await organizationRepository.SaveChangesAsync();
        }


    }
}
