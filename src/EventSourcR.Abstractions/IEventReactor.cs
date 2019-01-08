using System;

namespace EventSourcR
{
    public interface IEventReactor
    {
        IObservable<IRecordedEvent> EventStream();
        IObservable<IRecordedEvent> EventStream<T>() where T : IEvent;
        IObservable<IRecordedEvent> AggregateEventStream<T>() where T : IAggregate;
        IObservable<IRecordedEvent> AggregateEventStream<T>(Guid id) where T : IAggregate;
    }
}
