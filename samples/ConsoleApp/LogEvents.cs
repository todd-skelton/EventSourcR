using ConsoleBuildR;
using EventSourcR;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class LogEvents : IExecutable
    {
        private readonly IEventStore _eventStore;

        public LogEvents(IEventStore eventStore)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public async Task Execute(string[] args)
        {
            foreach (var @event in await _eventStore.GetEvents(1, 100))
            {
                Console.WriteLine(@event);
            }
        }
    }
}
