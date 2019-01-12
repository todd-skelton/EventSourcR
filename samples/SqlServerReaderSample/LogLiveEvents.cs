using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerReaderSample
{
    public class LogLiveEventsReactively : IExecutable
    {
        private readonly IEventReactor _reactor;

        public LogLiveEventsReactively(IEventReactor reactor)
        {
            _reactor = reactor ?? throw new ArgumentNullException(nameof(reactor));
        }

        public async Task Execute(string[] args)
        {
            Console.WriteLine("Subscribing to new events");

            var counter = 0;

            var stopWatch = new Stopwatch();

            var sub = _reactor.AggregateEventStream<ShoppingCart>().Subscribe(e =>
            {
                stopWatch.Restart();
                Console.Write($"\r{++counter}");
            });

            while (stopWatch.Elapsed < TimeSpan.FromMinutes(1))
            {
                await Task.Delay(50);
            }

            Console.WriteLine("");
            Console.WriteLine("No new events detected for 1 minute. Shutting down...");

            sub.Dispose();
        }
    }

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
                    // if the event is recieved out of order, start the query over.
                    if (currentEvent + 1 != @event.EventNumber) break;

                    Console.Write($"\r{++counter}");

                    currentEvent++;
                }
            }

            Console.WriteLine("");
            Console.WriteLine("No new events detected for 1 minute. Shutting down...");
        }
    }
}
