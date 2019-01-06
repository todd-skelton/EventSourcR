using System;

namespace EventSourcR
{
    public interface IRecordedEvent
    {
        long EventNumber { get; }
        Guid EventId { get; }
        string EventType { get; }
        Guid AggregateId { get; }
        string AggregateType { get; }
        long AggregateVersion { get; }
        IEvent Data { get; }
        IMetadata Metadata { get; }
        DateTimeOffset Recorded { get; }
    }
}
