using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace EventSourcR.SqlServer.Reactive
{
    public class Class1
    {
    }

    public class EventReactor : IEventReactor, IDisposable
    {
        private readonly ITypeMapper _typeMapper;
        private readonly IEventSerializer _serializer;
        private readonly EventStoreOptions _options;
        private readonly SqlTableDependency<ReactiveEventEntity> _dependency;
        private readonly Subject<IRecordedEvent> _subject = new Subject<IRecordedEvent>();

        public EventReactor(EventStoreOptions options, IEventSerializer serializer, ITypeMapper typeMapper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(serializer));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
            _dependency = new SqlTableDependency<ReactiveEventEntity>(options.ConnectionString, tableName: "Events", executeUserPermissionCheck: false);
            _dependency.OnChanged += Changed;
            _dependency.Start();
        }

        public IObservable<IRecordedEvent> Events() => _subject;

        public IObservable<IRecordedEvent> Events<T>() where T : IEvent => _subject.Where(e => e.Data is T);

        public IObservable<IRecordedEvent> AggregateEvents<T>() where T : IAggregate => _subject.Where(e => e.AggregateType == _typeMapper.GetAggregateName<T>());

        public IObservable<IRecordedEvent> AggregateEvents<T>(Guid id) where T : IAggregate => _subject.Where(e => e.AggregateId == id);

        public void Dispose()
        {
            _dependency.Stop();
            _dependency.Dispose();
        }

        public void Changed(object sender, RecordChangedEventArgs<ReactiveEventEntity> e) => _subject.OnNext(Transform(e.Entity));

        private IRecordedEvent Transform(ReactiveEventEntity entity)
        {
            return new RecordedEvent(
                    entity.EventNumber,
                    entity.EventId,
                    entity.EventType,
                    entity.AggregateId,
                    entity.AggregateType,
                    entity.AggregateVersion,
                    _serializer.Deserialize(entity.SerializedData, _typeMapper.GetEventType(entity.EventType)) as IEvent,
                    _serializer.Deserialize(entity.SerializedMetadata, _typeMapper.GetMetadataType(entity.EventType)) as IMetadata,
                    entity.Recorded
                );
        }
    }

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

        public DateTimeOffset Recorded { get; set; }
    }
}
