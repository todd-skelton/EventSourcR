using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        public Task Execute(string[] args)
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
                Thread.Sleep(100);
            }

            Console.WriteLine("");
            Console.WriteLine("No new events detected for 1 minute. Shutting down...");

            sub.Dispose();

            return Task.CompletedTask;
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

            long currentEvent = 364064;

            while(stopWatch.Elapsed < TimeSpan.FromMinutes(1))
            {
                var events = await _store.GetEvents(currentEvent + 1, 100);

                var last = events.LastOrDefault();

                if (last is null)
                {
                    Thread.Sleep(50);
                    continue;
                }

                stopWatch.Restart();

                currentEvent = last.EventNumber;

                foreach(var @event in events)
                {
                    Console.WriteLine(@event.Data);
                }
            }
            Console.WriteLine("No new events detected for 1 minute. Shutting down...");
        }
    }
}
