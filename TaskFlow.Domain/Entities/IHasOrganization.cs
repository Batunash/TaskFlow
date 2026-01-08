using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public interface IHasOrganization
    {
        int OrganizationId { get; set; }
    }
}
