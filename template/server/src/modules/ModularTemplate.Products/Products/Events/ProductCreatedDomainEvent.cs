using Bondstone.DomainEvents;
using ModularTemplate.SharedKernel.Domain;

namespace ModularTemplate.Products.Products.Events;

[DomainEventIdentity("products.product-created")]
public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string Name) : DomainEvent;
