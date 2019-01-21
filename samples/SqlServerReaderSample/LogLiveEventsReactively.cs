using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using System;
using System.Diagnostics;
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

            Console.WriteLine("\nNo new events detected for 1 minute. Shutting down...");

            sub.Dispose();
        }
    }
}
