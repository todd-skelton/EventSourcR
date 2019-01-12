using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using SqlServerSample.ShoppingCarts.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerSample
{
    public class EventStoreThroughput : IExecutable
    {
        private readonly IEventStore _store;
        private readonly IPendingEventFactory _factory;

        public EventStoreThroughput(IEventStore store, IPendingEventFactory factory)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task Execute(string[] args)
        {
            // run to establish an initial connection.
            await Task.Run(() =>
            {
                var id = Guid.NewGuid();
                var pendingEvent = _factory.Create(new ShoppingCart(id), new ShoppingCartCreated(Guid.NewGuid()));
                return _store.Append(id, 0, new[] { pendingEvent });
            });

            Console.WriteLine("=====================================\n");
            Console.WriteLine("Starting event store throughtput test.\n");

            var stopWatch = new Stopwatch();

            var tasks = new List<Task>();

            var max = 10000;

            Console.WriteLine($"Testing {max} events.\n");
            stopWatch.Start();
            for (var x = 0; x < max; x++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var id = Guid.NewGuid();
                    var pendingEvent = _factory.Create(new ShoppingCart(id), new ShoppingCartCreated(Guid.NewGuid()));
                    return _store.Append(id, 0, new[] { pendingEvent });
                }));
            }

            var masterTask = Task.WhenAll(tasks);

            while (!masterTask.IsCompleted)
            {
                var numberCompleted = tasks.Where(e => e.IsCompleted).Count();
                Console.Write($"\r{numberCompleted}");
            }

            stopWatch.Stop();

            var time = stopWatch.Elapsed;

            Console.WriteLine($" shopping carts took {time.TotalSeconds:#.##} seconds.\n");

            Console.WriteLine($"Throughput: {max / time.TotalSeconds:#.##} per second.\n");
            Console.WriteLine("=====================================\n");
        }
    }
}
