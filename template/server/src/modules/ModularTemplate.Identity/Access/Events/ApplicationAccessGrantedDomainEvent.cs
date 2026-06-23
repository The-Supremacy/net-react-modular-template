using Bondstone.DomainEvents;
using ModularTemplate.SharedKernel.Domain;

namespace ModularTemplate.Identity.Access.Events;

[DomainEventIdentity("identity.application-access-granted")]
public sealed record ApplicationAccessGrantedDomainEvent(
    Guid ApplicationAccessId,
    Guid LocalUserId) : DomainEvent;
