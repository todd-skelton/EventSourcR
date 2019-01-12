using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using SqlServerSample.ShoppingCarts.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServerSample
{
    public class RepositoryThroughput : IExecutable
    {
        private readonly IRepository<ShoppingCart> _repository;

        public RepositoryThroughput(IRepository<ShoppingCart> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Execute(string[] args)
        {
            // run to establish an initial connection.
            await Task.Run(() =>
            {
                var id = Guid.NewGuid();

                var shoppingCart = new ShoppingCart(id);

                var createShoppingCart = new CreateShoppingCart(Guid.NewGuid());

                shoppingCart.Handle(createShoppingCart);

                return _repository.Save(shoppingCart);
            });


            Console.WriteLine("=====================================\n");
            Console.WriteLine("Starting repository throughtput test.\n");

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

                    var shoppingCart = new ShoppingCart(id);

                    var createShoppingCart = new CreateShoppingCart(Guid.NewGuid());

                    shoppingCart.Handle(createShoppingCart);

                    return _repository.Save(shoppingCart);
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
