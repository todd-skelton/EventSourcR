using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcR
{
    /// <summary>
    /// Stores events.
    /// </summary>
    public interface IEventStore
    {
        Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount);
        Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent;
        Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate;
        Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate;
        Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents);
    }

    public interface IEventWriter
    {
        Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents);
    }

    public interface IEventReader
    {
        Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount);
        Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent;
        Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate;
        Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate;
    }

    public interface IEventReactor
    {
        IObservable<IRecordedEvent> Events();
        IObservable<IRecordedEvent> Events<T>() where T : IEvent;
        IObservable<IRecordedEvent> AggregateEvents<T>() where T : IAggregate;
        IObservable<IRecordedEvent> AggregateEvents<T>(Guid id) where T : IAggregate;
    }
}
