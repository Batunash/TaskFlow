using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TaskFlow.Tests.InfrastructureTest
{
    public class NoModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            return Guid.NewGuid();
        }
    }
}