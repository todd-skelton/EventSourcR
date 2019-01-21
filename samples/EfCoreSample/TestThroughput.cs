using EfCoreSample.Orders;
using EfCoreSample.Orders.Commands;
using ConsoleBuildR;
using EventSourcR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EfCoreSample
{
    public class TestThroughput : IExecutable
    {
        private readonly IServiceProvider _provider;

        public TestThroughput(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task Execute(string[] args)
        {
            var stopWatch = new Stopwatch();
            var rand = new Random();
            var tasks = new List<Task>();

            stopWatch.Start();
            for (var x = 0; x < 1000; x++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetService<IRepository<Order>>();

                        var id = Guid.NewGuid();

                        var order = new Order(id);

                        string orderNo = rand.Next(100, 1000) + "-" + rand.Next(100, 1000);

                        var createOrder = new CreateOrder(orderNo);

                        order.Issue(createOrder);

                        await repository.Save(order);
                    }
                }));

            }
            await Task.WhenAll(tasks);
            stopWatch.Stop();

            var time = stopWatch.Elapsed;

            Console.WriteLine($"1000 Orders took {time}");

            Console.WriteLine($"Throughput: {1000 / time.TotalSeconds} per second");
        }
    }
}
