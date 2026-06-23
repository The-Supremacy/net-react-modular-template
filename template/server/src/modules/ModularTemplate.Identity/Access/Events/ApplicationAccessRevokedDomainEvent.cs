using Bondstone.DomainEvents;
using ModularTemplate.SharedKernel.Domain;

namespace ModularTemplate.Identity.Access.Events;

[DomainEventIdentity("identity.application-access-revoked")]
public sealed record ApplicationAccessRevokedDomainEvent(
    Guid ApplicationAccessId,
    Guid LocalUserId) : DomainEvent;
