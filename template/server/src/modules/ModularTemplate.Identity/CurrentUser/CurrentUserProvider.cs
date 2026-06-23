using ModularTemplate.Identity.Contracts.CurrentUser;

namespace ModularTemplate.Identity.CurrentUser;

public sealed class CurrentUserProvider(
    SynchronizeCurrentUserCommandHandler handler)
    : ICurrentUserProvider
{
    public async Task<CurrentUserContext> GetCurrentUserAsync(
        AuthenticatedIdentity? identity,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(
            new SynchronizeCurrentUserCommand(identity),
            cancellationToken);
    }
}
