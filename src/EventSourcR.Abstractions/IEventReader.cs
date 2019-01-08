using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcR
{
    public interface IEventReader
    {
        Task<long> GetLastestEventNumber();
        Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount);
        Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent;
        Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate;
        Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate;
    }
}
