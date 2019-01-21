using System;
using System.Collections.Generic;

namespace EventSourcR
{
    public abstract class AggregateBase<T> : IAggregate<T> where T : IAggregate<T>
    {
        protected IList<IEvent<T>> _pendingEvents = new List<IEvent<T>>();

        protected AggregateBase(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
        public long Version { get; private set; }
        public long PendingVersion { get; private set; }

        public virtual IEnumerable<IEvent<T>> PendingEvents => _pendingEvents.AsReadOnly();

        public abstract void Execute<TCommand>(TCommand command) where TCommand : ICommand<T>;

        public virtual void Apply<TEvent>(TEvent @event) where TEvent : IEvent<T>
        {
            Handle(@event);
            Version++;
            PendingVersion++;
        }

        public virtual void ClearPendingEvents()
        {
            Version = PendingVersion;
            _pendingEvents.Clear();
        }

        protected abstract void Handle<TEvent>(TEvent @event) where TEvent : IEvent<T>;

        protected virtual void RaiseEvent<TEvent>(TEvent @event) where TEvent : IEvent<T>
        {
            Handle(@event);
            _pendingEvents.Add(@event);
            PendingVersion++;
        }
    }
}
