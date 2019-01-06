using System.Threading.Tasks;

namespace EventSourcR
{
    /// <summary>
    /// Save and retrieve aggregates.
    /// </summary>
    /// <typeparam name="T">The type of aggregate being stored.</typeparam>
    public interface IRepository<T> where T : IAggregate<T>
    {
        /// <summary>
        /// Loads the aggregate with the events from the event store.
        /// </summary>
        /// <param name="aggregate">The aggregate to load with its id set.</param>
        /// <param name="toVersion">Aggregate version to end with.</param>
        /// <returns>The loaded aggregate.</returns>
        Task<T> Load(T aggregate, long? toVersion = null);
        Task Save(T aggregate);
    }
}
