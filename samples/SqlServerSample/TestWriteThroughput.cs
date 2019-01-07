using ConsoleBuildR;
using EventSourcR;
using EventSourcR.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SqlServerSample.ShoppingCarts;
using SqlServerSample.ShoppingCarts.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlServerSample
{
    public class TestWriteThroughput : IExecutable
    {
        private readonly IEventStore _store;
        private readonly IServiceProvider _provider;
        private readonly IEventReactor _reactor;
        private IDictionary<Guid, ShoppingCart> _carts = new Dictionary<Guid, ShoppingCart>();

        public TestWriteThroughput(IEventStore store, IServiceProvider provider, IEventReactor reactor)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _reactor = reactor ?? throw new ArgumentNullException(nameof(reactor));
        }

        public async Task Execute(string[] args)
        {
            Console.WriteLine("Building application state in memory.");

            var allEvents = await _store.GetAggregateEvents<ShoppingCart>(1, int.MaxValue);

            Console.WriteLine($"{allEvents.Count()} events to process.");

            foreach (var events in allEvents.GroupBy(e => e.AggregateId))
            {
                var cart = new ShoppingCart(events.Key);

                _carts[events.Key] = events.Select(e => e.Data as IEvent<ShoppingCart>).BuildState(cart);
            }

            Console.WriteLine($"{_carts.Count} carts in memory.");

            Console.WriteLine("Subscribing to new events");
            var sub = _reactor.AggregateEvents<ShoppingCart>().Subscribe(e=> 
            {
                if (_carts.ContainsKey(e.AggregateId))
                {
                    _carts[e.AggregateId].Apply(e.Data as IEvent<ShoppingCart>);
                }
                else
                {
                    var cart = new ShoppingCart(e.AggregateId);
                    cart.Apply(e as IEvent<ShoppingCart>);
                    _carts[e.AggregateId] = cart;
                }
            });

            var stopWatch = new Stopwatch();

            var tasks = new List<Task>();

            var max = 10;

            Console.WriteLine($"Testing {max} events");
            stopWatch.Start();
            for (var x = 0; x < max; x++)
            {
                var task = Task.Run(async () =>
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetService<IRepository<ShoppingCart>>();

                        var id = Guid.NewGuid();

                        var shoppingCart = new ShoppingCart(id);

                        var createShoppingCart = new CreateShoppingCart(Guid.NewGuid());

                        shoppingCart.Handle(createShoppingCart);

                        await repository.Save(shoppingCart);
                    }
                });
                  
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();
            var time = stopWatch.Elapsed;

            Console.WriteLine($"{max} shopping carts took {time.TotalSeconds:#.##} seconds.");

            Console.WriteLine($"Throughput: {max / time.TotalSeconds:#.##} per second");

            Thread.Sleep(5000);

            Console.WriteLine($"{_carts.Count} carts now in memory.");

            Console.ReadLine();

            sub.Dispose();

            return;
        }
    }
}
