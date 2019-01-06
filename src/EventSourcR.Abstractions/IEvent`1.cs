namespace EventSourcR
{
    /// <summary>
    /// An event.
    /// </summary>
    /// <typeparam name="T">The type of stream this event belongs to.</typeparam>
    public interface IEvent<T> : IEvent where T : IAggregate<T>
    {

    }
}
