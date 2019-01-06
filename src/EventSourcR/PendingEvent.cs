using System;

namespace EventSourcR
{
    public class PendingEvent : IPendingEvent
    {
        public PendingEvent(Guid id, string type, Guid aggregateId, string aggregateType, IEvent data, IMetadata metaData)
        {
            EventId = id;
            EventType = type ?? throw new ArgumentNullException(nameof(type));
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Metadata = metaData ?? throw new ArgumentNullException(nameof(metaData));
        }

        public Guid EventId { get; }

        public string EventType { get; }

        public Guid AggregateId { get; }

        public string AggregateType { get; }

        public IEvent Data { get; }

        public IMetadata Metadata { get; }
    }
}
