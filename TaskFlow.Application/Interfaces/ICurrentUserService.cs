using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
    }
}
