using System;

namespace EventSourcR
{
    public interface IPendingEvent
    {
        Guid EventId { get; }
        string EventType { get; }
        Guid AggregateId { get; }
        string AggregateType { get; }
        IEvent Data { get; }
        IMetadata Metadata { get; }
    }
}
