using ConsoleBuildR;
using EventSourcR;
using Microsoft.Extensions.DependencyInjection;
using SqlServerSample.ShoppingCarts;
using SqlServerSample.ShoppingCarts.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlServerSample
{
    public class TestWriteThroughput : IExecutable
    {
        private readonly IServiceProvider _provider;

        public TestWriteThroughput(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task Execute(string[] args)
        {
            var stopWatch = new Stopwatch();

            var tasks = new List<Task>();

            var max = 100;

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
        }
    }
}
