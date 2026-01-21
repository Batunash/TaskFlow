using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Services
{
    public class OrganizationService(IOrganizationRepository organizationRepository, ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService, IUserRepository userRepository,
        IValidator<CreateOrganizationDto> createOrgValidator, IValidator<InviteUserDto> inviteUserValidator
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
            if (!organization.IsOwner(currentUserId))
            {
                var member = organization.Members.FirstOrDefault(m => m.UserId == currentUserId);
                if (member == null || !member.IsAccepted)
                {
                    throw new UnauthorizedAccessException("User is not an active member of this organization.");
                }
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
            var userToInvite = await userRepository.GetByUserNameAsync(dto.UserName);
            if (userToInvite == null)
            {
                throw new NotFoundException($"User '{dto.UserName}' not found.");
            }
            organization.AddMember(userToInvite.Id, OrganizationRole.Member);

            await organizationRepository.SaveChangesAsync();
        }

        public async Task AcceptInvitationAsync(int organizationId, int currentUserId)
        {
            var organization = await organizationRepository.GetByIdWithMembersAsync(organizationId)
                 ?? throw new NotFoundException("Organization not found.");

            var member = organization.Members.FirstOrDefault(m => m.UserId == currentUserId);

            if (member == null)
            {
                throw new UnauthorizedAccessException("No invitation found for this organization.");
            }

            if (member.IsAccepted)
            {
                throw new BusinessRuleException("You are already a member.");
            }

            member.AcceptInvitation();

            var user = await userRepository.GetByIdAsync(currentUserId);
            if (user != null)
            {
                user.OrganizationId = organization.Id;
            }
            await organizationRepository.SaveChangesAsync();
        }
        public async Task<List<UserDto>> GetMembersAsync(int organizationId)
        {
            var organization = await organizationRepository.GetByIdWithMembersAsync(organizationId);

            if (organization == null)
            {
                throw new NotFoundException($"Organization with ID {organizationId} not found.");
            }
            var membersDto = new List<UserDto>();
            foreach (var member in organization.Members)
            {
                if (member.IsAccepted) 
                {
                    var user = await userRepository.GetByIdAsync(member.UserId);
                    if (user != null)
                    {
                        membersDto.Add(new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName
                        });
                    }
                }
            }
            
            return membersDto;
        }

        public async Task<List<OrganizationInvitationDto>> GetMyInvitationsAsync(int currentUserId)
        {
            var pendingOrgs = await organizationRepository.GetPendingInvitationsByUserIdAsync(currentUserId);

            return pendingOrgs.Select(o => new OrganizationInvitationDto
            {
                OrganizationId = o.Id,
                OrganizationName = o.Name,
                Role = o.Members.First(m => m.UserId == currentUserId).Role.ToString()
            }).ToList();
        }
    }
}