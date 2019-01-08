namespace EventSourcR
{
    /// <summary>
    /// Stores events.
    /// </summary>
    public interface IEventStore : IEventReader, IEventWriter
    {

    }
}
