using Bondstone.DomainEvents;
using ModularTemplate.SharedKernel.Domain;

namespace ModularTemplate.Identity.Users.Events;

[DomainEventIdentity("identity.local-user-created")]
public sealed record LocalUserCreatedDomainEvent(
    Guid LocalUserId,
    string Provider,
    string Subject) : DomainEvent;
