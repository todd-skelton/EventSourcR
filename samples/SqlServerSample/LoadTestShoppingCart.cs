using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using System;
using System.Threading.Tasks;

namespace SqlServerSample
{
    public class LoadTestShoppingCart : IExecutable
    {
        private readonly IRepository<ShoppingCart> _repository;

        public LoadTestShoppingCart(IRepository<ShoppingCart> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Execute(string[] args)
        {
            // create blank entity with the ID I'm loading.
            var shoppingCart = new ShoppingCart(Guid.Parse("1C8A0882-BE42-43BA-ACBD-CE347065A6CC"));

            // load the past events for the ID of the blank shopping cart if any
            await _repository.Load(shoppingCart);

            Console.WriteLine("Shopping Cart:");
            Console.WriteLine();
            Console.WriteLine("Price\tQty\tName");
            foreach(var product in shoppingCart.Products.Values)
            {
                Console.WriteLine($"{product.Price:c}\t{product.Quantity}\t{product.Name}");
            }
            Console.WriteLine();
            Console.WriteLine($"Total: {shoppingCart.Total:c}");
        }
    }
}
