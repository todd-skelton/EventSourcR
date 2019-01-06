using ConsoleApp.Orders;
using ConsoleApp.Orders.Commands;
using ConsoleBuildR;
using EventSourcR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class TestThroughput : IExecutable
    {
        private readonly IServiceProvider _provider;

        public TestThroughput(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Task Execute(string[] args)
        {
            var start = DateTimeOffset.Now;
            var rand = new Random();
            for (var x = 0; x < 1000; x++)
            {
                using (var scope = _provider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetService<IRepository<Order>>();

                    var id = Guid.NewGuid();

                    var order = new Order(id);

                    string orderNo = rand.Next(100, 1000) + "-" + rand.Next(100, 1000);

                    var createOrder = new CreateOrder(orderNo);

                    order.Handle(createOrder);

                    repository.Save(order);
                }
            }

            var end = DateTimeOffset.Now;

            var time = end - start;

            Console.WriteLine($"1000 Orders took {time}");

            Console.WriteLine($"Throughput: {1000 / time.TotalSeconds} per second");

            return Task.CompletedTask;
        }
    }
}
