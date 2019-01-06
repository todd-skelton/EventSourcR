﻿using System;
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

        public virtual void BuildState(IEnumerable<IEvent<T>> events)
        {
            foreach (var @event in events)
            {
                Apply(@event);
                Version++;
            }
            PendingVersion = Version;
        }

        public virtual void ClearPendingEvents()
        {
            Version = PendingVersion;
            _pendingEvents.Clear();
        }

        public abstract void Handle(ICommand<T> command);

        protected abstract void Handle(IEvent<T> @event);

        protected virtual void RaiseEvent(IEvent<T> @event)
        {
            Apply(@event);
            _pendingEvents.Add(@event);
        }

        private void Apply(IEvent<T> @event)
        {
            Handle(@event);
            PendingVersion++;
        }
    }
}