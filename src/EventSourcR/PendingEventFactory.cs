using System;

namespace EventSourcR
{
    public class PendingEventFactory : IPendingEventFactory
    {
        private readonly ITypeMapper _typeMapper;

        public PendingEventFactory(ITypeMapper typeMapper)
        {
            _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));
        }

        public IPendingEvent Create<T>(T aggregate, IEvent @event) where T : IAggregate<T>
        {
            return new PendingEvent(Guid.NewGuid(), _typeMapper.GetEventName(@event.GetType()), aggregate.Id, _typeMapper.GetAggregateName<T>(), @event, new EmptyMetadata()); 
        }
    }
}
