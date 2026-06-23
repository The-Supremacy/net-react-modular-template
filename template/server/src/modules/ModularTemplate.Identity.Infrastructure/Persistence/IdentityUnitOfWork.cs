using ModularTemplate.Identity;

namespace ModularTemplate.Identity.Infrastructure.Persistence;

public sealed class IdentityUnitOfWork(IdentityDbContext dbContext) : IIdentityUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
