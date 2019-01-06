using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcR.EntityFrameworkCore
{
    public class EventStore : DbContext, IEventStore
    {
        private readonly ITypeMapper _typeMapper;
        private readonly IEventSerializer _eventSerializer;

        public EventStore(ITypeMapper typeMapper, IEventSerializer eventSerializer, DbContextOptions<EventStore> options) : base(options)
        {
            _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
            _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        }

        private DbSet<RecordedEventEntity> Events { get; set; }

        public Task Append(Guid aggregateId, long expectedAggregateVersion, IEnumerable<IPendingEvent> pendingEvents)
        {
            if (!pendingEvents.Any()) return Task.CompletedTask;

            foreach (var @event in pendingEvents)
            {
                var recordedEvent = new RecordedEventEntity(@event.EventId, @event.EventType, aggregateId, @event.AggregateType, ++expectedAggregateVersion, _eventSerializer.Serialize(@event.Data), _eventSerializer.Serialize(@event.Metadata));

                Events.Add(recordedEvent);
            }

            return SaveChangesAsync();
        }

        public Task<IEnumerable<IRecordedEvent>> GetEvents(long fromEventNumber, int maxCount)
        {
            return Task.FromResult(Transform(Events.Where(e => e.EventNumber >= fromEventNumber).Take(maxCount)));
        }

        public Task<IEnumerable<IRecordedEvent>> GetEvents<T>(long fromEventNumber, int maxCount) where T : IEvent
        {
            return Task.FromResult(Transform(Events.Where(e => e.EventNumber >= fromEventNumber && e.EventType == _typeMapper.GetEventName<T>()).Take(maxCount)));
        }

        public Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(long fromEventNumber, int maxCount) where T : IAggregate
        {
            return Task.FromResult(Transform(Events.Where(e => e.EventNumber >= fromEventNumber && e.AggregateType == _typeMapper.GetAggregateName<T>()).Take(maxCount)));
        }

        public Task<IEnumerable<IRecordedEvent>> GetAggregateEvents<T>(Guid id, long fromAggregateVersion, int maxCount) where T : IAggregate
        {
            return Task.FromResult(Transform(Events.Where(e => e.AggregateId == id && e.AggregateVersion >= fromAggregateVersion).Take(maxCount)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecordedEventEntity>(b =>
            {
                b.HasKey(e => e.EventNumber);
                b.HasAlternateKey(e => e.EventId);
                b.HasAlternateKey(e => new { e.AggregateId, e.AggregateVersion });
                b.HasIndex(e => e.EventType);
                b.HasIndex(e => e.AggregateType);
                b.HasIndex(e => e.AggregateId);
                b.Property(e => e.EventNumber).ValueGeneratedOnAdd();
            });
        }

        private IEnumerable<IRecordedEvent> Transform(IEnumerable<RecordedEventEntity> entities)
        {
            foreach (var entity in entities)
            {
                yield return new RecordedEvent(
                    entity.EventNumber,
                    entity.EventId,
                    entity.EventType,
                    entity.AggregateId,
                    entity.AggregateType,
                    entity.AggregateVersion,
                    _eventSerializer.Deserialize(entity.SerializedData, _typeMapper.GetEventType(entity.EventType)) as IEvent,
                    _eventSerializer.Deserialize(entity.SerializedMetadata, _typeMapper.GetMetadataType(entity.EventType)) as IMetadata,
                    entity.Recorded
                );
            }
        }
    }
}