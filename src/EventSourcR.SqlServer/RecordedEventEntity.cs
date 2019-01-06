using System;

namespace EventSourcR.SqlServer
{
    public class RecordedEventEntity
    {
        public RecordedEventEntity(Guid eventId, string eventType, Guid aggregateId, string aggregateType, long aggregateVersion, string serializedData, string serializedMetadata)
        {
            EventId = eventId;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            AggregateId = aggregateId;
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
            AggregateVersion = aggregateVersion;
            SerializedData = serializedData ?? throw new ArgumentNullException(nameof(serializedData));
            SerializedMetadata = serializedMetadata ?? throw new ArgumentNullException(nameof(serializedMetadata));
            Recorded = DateTimeOffset.Now;
        }

        public long EventNumber { get; private set; }

        public Guid EventId { get; private set; }

        public string EventType { get; private set; }

        public Guid AggregateId { get; private set; }

        public string AggregateType { get; private set; }

        public long AggregateVersion { get; private set; }

        public string SerializedData { get; private set; }

        public string SerializedMetadata { get; private set; }

        public DateTimeOffset Recorded { get; private set; }
    }
}
