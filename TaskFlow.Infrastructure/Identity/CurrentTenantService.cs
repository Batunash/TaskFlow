using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TaskFlow.Application.Interfaces;
namespace TaskFlow.Infrastructure.Identity
{
    public class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenantService
    {
        public int? OrganizationId
        {
            get
            {
                var claim = httpContextAccessor.HttpContext?.User?.FindFirst("organizationId");
                return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
            }
        }
    }
}
