using System;

namespace EventSourcR.EntityFrameworkCore
{
    public class RecordedEvent : IRecordedEvent
    {
        public RecordedEvent(long eventNumber, Guid eventId, string eventType, Guid aggregateId, string aggregateType, long aggregateVersion, IEvent data, IMetadata metadata, DateTimeOffset recorded)
        {
            EventNumber = eventNumber;
            EventId = eventId;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            AggregateVersion = aggregateVersion;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Recorded = recorded;
        }

        public long EventNumber { get; private set; }

        public Guid EventId { get; private set; }

        public string EventType { get; private set; }

        public Guid AggregateId { get; private set; }

        public string AggregateType { get; private set; }

        public long AggregateVersion { get; private set; }

        public IEvent Data { get; private set; }

        public IMetadata Metadata { get; private set; }

        public DateTimeOffset Recorded { get; private set; }
    }
}