using System.Collections.Generic;

namespace EventSourcR.Extensions
{
    public static class EventExtensions
    {
        public static T BuildState<T>(this IEnumerable<IEvent<T>> events, T aggregate) where T : IAggregate<T>
        {
            foreach(var @event in events)
            {
                aggregate.Apply(@event);
            }
            return aggregate;
        }
    }
}