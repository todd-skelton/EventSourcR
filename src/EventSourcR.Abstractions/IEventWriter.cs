using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcR
{
    public interface IEventWriter
    {
        Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents);
    }
}
