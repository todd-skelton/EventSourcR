using System.Collections.Generic;

namespace EventSourcR
{
    /// <summary>
    /// An aggregate root.
    /// </summary>
    /// <typeparam name="T">The type of stream the aggregate belongs to.</typeparam>
    public interface IAggregate<T> : IAggregate where T : IAggregate<T>
    {
        long PendingVersion { get; }
        IEnumerable<IEvent<T>> PendingEvents { get; }
        void Execute<TCommand>(TCommand command) where TCommand : ICommand<T>;
        void Apply<TEvent>(TEvent events) where TEvent : IEvent<T>;
        void ClearPendingEvents();
    }
}
