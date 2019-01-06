using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EventSourcR.SqlServer
{
    public class EventStore : IEventStore
    {
        private const string _eventQueryString = "SELECT [Offset], [Id], [Type], [AggregateId], [AggregateType], [AggregateVersion], [SerializedEvent] FROM [Events]";
        private readonly ITypeMapper _typeMapper;
        private readonly IEventSerializer _serializer;
        private readonly EventStoreOptions _options;

        public EventStore(ITypeMapper typeMapper, IEventSerializer serializer, EventStoreOptions options)
        {
            _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents)
        {
            return Task.Run(() =>
            {
                using (var connection = new SqlConnection(_options.ConnectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var @event in pendingEvents)
                        {
                            var command = new SqlCommand(@"INSERT INTO Events VALUES(@eventId, @eventType, @aggregateId, @aggregateType, @aggregateVersion, @serializedData, @serializedMetadata, @recorded)", connection, transaction);
                            command.Parameters.AddWithValue("@eventId", Guid.NewGuid());
                            command.Parameters.AddWithValue("@eventType", @event.EventType);
                            command.Parameters.AddWithValue("@aggregateId", aggregateId);
                            command.Parameters.AddWithValue("@aggregateType", @event.AggregateType);
                            command.Parameters.AddWithValue("@aggregateVersion", ++expectedAggregateVersion);
                            command.Parameters.AddWithValue("@serializedData", _serializer.Serialize(@event.Data));
                            command.Parameters.AddWithValue("@serializedMetadata", _serializer.Serialize(@event.Metadata));
                            command.Parameters.AddWithValue("@recorded", DateTimeOffset.Now);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            });
        }

        public virtual Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent
        {
            throw new NotImplementedException();
        }
    }
}
