namespace ModularTemplate.Identity;

public interface IIdentityUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
