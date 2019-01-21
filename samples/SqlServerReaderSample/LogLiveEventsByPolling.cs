using ConsoleBuildR;
using EventSourcR;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerReaderSample
{
    public class LogLiveEventsByPolling : IExecutable
    {
        private readonly IEventStore _store;

        public LogLiveEventsByPolling(IEventStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task Execute(string[] args)
        {
            Console.WriteLine("Subscribing to new events");

            var stopWatch = new Stopwatch();

            var counter = 0;

            long currentEvent = await _store.GetLastestEventNumber();

            while(stopWatch.Elapsed < TimeSpan.FromMinutes(1))
            {
                var events = await _store.GetEvents(currentEvent + 1, 100);

                var last = events.LastOrDefault();

                if (last is null)
                {
                    await Task.Delay(50);
                    continue;
                }

                stopWatch.Restart();

                foreach(var @event in events)
                {
                    Console.Write($"\r{++counter}");

                    currentEvent++;
                }
            }

            Console.WriteLine("\nNo new events detected for 1 minute. Shutting down...");
        }
    }
}
