using System;
using System.Runtime.Serialization;

namespace EventSourcR.SqlServer.Reactive
{
    public class ReactiveEventEntity
    {
        public long EventNumber { get; set; }

        public Guid EventId { get; set; }

        public string EventType { get; set; }

        public Guid AggregateId { get; set; }

        public string AggregateType { get; set; }

        public long AggregateVersion { get; set; }

        public string SerializedData { get; set; }

        public string SerializedMetadata { get; set; }

        public string Recorded { get; set; }
    }
}
