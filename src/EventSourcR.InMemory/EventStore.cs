using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcR.InMemory
{
    public class EventStore : IEventStore
    {
        private readonly IList<IRecordedEvent> _events = new List<IRecordedEvent>();
        private readonly ITypeMapper _typeMapper;

        public EventStore(ITypeMapper typeMapper)
        {
            _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
        }

        public Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents)
        {
            if (!pendingEvents.Any()) return Task.CompletedTask;

            var currentAggregateVersion = _events.LastOrDefault(e => e.AggregateId == aggregateId)?.AggregateVersion ?? 0;

            if (currentAggregateVersion != expectedAggregateVersion) throw new ArgumentException($"Can't append events. Current version is {currentAggregateVersion} and expected version is {expectedAggregateVersion}.");

            foreach (var @event in pendingEvents)
            {
                var recordedEvent = new RecordedEvent(_events.Count, @event.EventId, @event.EventType, aggregateId, @event.AggregateType, ++currentAggregateVersion, @event.Data, @event.Metadata);

                _events.Add(recordedEvent);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount)
        {
            return Task.FromResult(_events.Where(e => e.EventNumber >= fromEventNumber).Take(maxCount));
        }

        public Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent
        {
            return Task.FromResult(_events.Where(e => e.EventType == _typeMapper.GetEventName<T>() && e.EventNumber >= fromEventNumber).Take(maxCount));
        }

        public Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate
        {
            return Task.FromResult(_events.Where(e => e.AggregateType == _typeMapper.GetAggregateName<T>() && e.EventNumber >= fromEventNumber).Take(maxCount));
        }

        public Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate
        {
            return Task.FromResult(_events.Where(e => e.AggregateId == id && e.AggregateVersion >= fromAggregateVersion).Take(maxCount));
        }
    }
}
