using ModularTemplate.Identity;
using ModularTemplate.Identity.Access;
using ModularTemplate.Identity.Contracts.CurrentUser;
using ModularTemplate.Identity.Users;

namespace ModularTemplate.Identity.CurrentUser;

public sealed record SynchronizeCurrentUserCommand(
    AuthenticatedIdentity? Identity);

public sealed class SynchronizeCurrentUserCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    ILocalUserRepository localUserRepository,
    IApplicationAccessRepository applicationAccessRepository)
{
    public async Task<CurrentUserContext> HandleAsync(
        SynchronizeCurrentUserCommand command,
        CancellationToken cancellationToken)
    {
        AuthenticatedIdentity? identity = command.Identity;
        if (identity is null || string.IsNullOrWhiteSpace(identity.Subject))
        {
            return CurrentUserContext.Unauthenticated;
        }

        LocalUser? localUser = await localUserRepository.GetByProviderSubjectAsync(
            identity.Provider,
            identity.Subject,
            cancellationToken);

        if (localUser is null)
        {
            localUser = LocalUser.Create(
                identity.Provider,
                identity.Subject,
                identity.DisplayName,
                identity.Email);
            localUserRepository.Add(localUser);
        }
        else
        {
            localUser.MarkSeen(identity.DisplayName, identity.Email);
        }

        bool hasAccess = await applicationAccessRepository.HasActiveAccessAsync(localUser.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CurrentUserContext(
            true,
            localUser.Id,
            localUser.DisplayName,
            localUser.Email?.Value,
            hasAccess);
    }
}
