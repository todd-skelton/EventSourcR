using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcR.EventStore
{
    public class EventStore : IEventStore
    {
        private readonly ITypeMapper _typeMapper;
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _serializer;

        public EventStore(ITypeMapper typeMapper, IEventStoreConnection connection, IEventSerializer serializer)
        {
            _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents)
        {
            if (!pendingEvents.Any()) return Task.CompletedTask;

            var aggregateType = pendingEvents.First().AggregateType;

            var eventDataList = new List<EventData>();
            foreach(var @event in pendingEvents)
            {
                eventDataList.Add(new EventData(Guid.NewGuid(), @event.EventType, true, Encoding.UTF8.GetBytes(_serializer.Serialize(@event.Data)), Encoding.UTF8.GetBytes(_serializer.Serialize(@event.Metadata))));
            }

            return _connection.AppendToStreamAsync(aggregateType + "-" + aggregateId, expectedAggregateVersion - 1, eventDataList);
        }

        public Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent
        {
            throw new NotImplementedException();
        }
    }
}
