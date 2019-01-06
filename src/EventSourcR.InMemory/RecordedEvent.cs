using System;

namespace EventSourcR.InMemory
{
    public class RecordedEvent : IRecordedEvent
    {
        public RecordedEvent(long index, Guid id, string type, Guid aggregateId, string aggregateType, long aggregateVersion, IEvent data, IMetadata metaData)
        {
            EventNumber = index;
            EventId = id;
            EventType = type ?? throw new ArgumentNullException(nameof(type));
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            AggregateVersion = aggregateVersion;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Metadata = metaData ?? throw new ArgumentNullException(nameof(metaData));
            Recorded = DateTimeOffset.Now;
        }

        public long EventNumber { get; }

        public Guid EventId { get; }

        public string EventType { get; }

        public Guid AggregateId { get; }

        public string AggregateType { get; }

        public long AggregateVersion { get; }

        public IEvent Data { get; }

        public IMetadata Metadata { get; }

        public DateTimeOffset Recorded { get; }
    }
}
