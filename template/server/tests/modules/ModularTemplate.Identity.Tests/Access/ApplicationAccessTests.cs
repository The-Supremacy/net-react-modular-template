using ModularTemplate.Identity.Access;
using ModularTemplate.Identity.Access.Events;
using Shouldly;

namespace ModularTemplate.Identity.Tests.Access;

public sealed class ApplicationAccessTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WhenAccessIsGranted_RecordsGrantedEvent()
    {
        var access = ApplicationAccess.GrantTo(Guid.NewGuid());

        access.IsActive.ShouldBeTrue();
        access.PendingDomainEvents.Single().ShouldBeOfType<ApplicationAccessGrantedDomainEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Revoke_WhenAccessIsActive_RecordsRevokedEvent()
    {
        var access = ApplicationAccess.GrantTo(Guid.NewGuid());
        access.ClearPendingDomainEvents();

        access.Revoke();

        access.IsActive.ShouldBeFalse();
        access.DisabledAt.ShouldNotBeNull();
        access.PendingDomainEvents.Single().ShouldBeOfType<ApplicationAccessRevokedDomainEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Grant_WhenAccessIsRevoked_RecordsGrantedEvent()
    {
        var access = ApplicationAccess.GrantTo(Guid.NewGuid());
        access.Revoke();
        access.ClearPendingDomainEvents();

        access.Grant();

        access.IsActive.ShouldBeTrue();
        access.DisabledAt.ShouldBeNull();
        access.PendingDomainEvents.Single().ShouldBeOfType<ApplicationAccessGrantedDomainEvent>();
    }
}
