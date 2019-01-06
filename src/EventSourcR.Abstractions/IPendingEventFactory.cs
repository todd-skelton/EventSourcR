namespace EventSourcR
{
    public interface IPendingEventFactory
    {
        IPendingEvent Create<T>(T aggregate, IEvent @event) where T : IAggregate<T>;
    }
}
