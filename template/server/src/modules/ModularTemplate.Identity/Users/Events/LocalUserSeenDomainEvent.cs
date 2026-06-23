using Bondstone.DomainEvents;
using ModularTemplate.SharedKernel.Domain;

namespace ModularTemplate.Identity.Users.Events;

[DomainEventIdentity("identity.local-user-seen")]
public sealed record LocalUserSeenDomainEvent(
    Guid LocalUserId,
    string Provider,
    string Subject) : DomainEvent;
