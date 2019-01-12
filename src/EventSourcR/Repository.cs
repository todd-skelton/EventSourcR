using EventSourcR.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcR
{
    public class Repository<T> : IRepository<T> where T : IAggregate<T>
    {
        private readonly IEventStore _eventStore;
        private readonly IPendingEventFactory _eventFactory;

        public Repository(IEventStore eventStore, IPendingEventFactory eventFactory)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _eventFactory = eventFactory ?? throw new ArgumentNullException(nameof(eventFactory));
        }

        public async Task<T> Load(T aggregate, long? toVersion = null)
        {

            int maxCount = toVersion.HasValue ? (int)(toVersion.Value - aggregate.Version) : int.MaxValue;

            var events = await _eventStore.GetAggregateEvents<T>(aggregate.Id, aggregate.Version + 1, maxCount);

            var data = events.Select(e => e.Data as IEvent<T>);

            return data.BuildState(aggregate);
        }

        public async Task Save(T aggregate)
        {
            await _eventStore.Append(aggregate.Id, aggregate.Version, GetPendingEvents(aggregate));
            aggregate.ClearPendingEvents();
        }

        private IEnumerable<IPendingEvent> GetPendingEvents(T aggregate)
        {
            foreach (var @event in aggregate.PendingEvents)
            {
                yield return _eventFactory.Create(aggregate, @event);
            }
        }
    }
}
