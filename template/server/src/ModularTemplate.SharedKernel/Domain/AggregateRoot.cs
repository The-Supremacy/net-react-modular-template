using Bondstone.DomainEvents;

namespace ModularTemplate.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IDomainEventSource
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> PendingDomainEvents => _domainEvents.AsReadOnly();

    public void ClearPendingDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }
}
