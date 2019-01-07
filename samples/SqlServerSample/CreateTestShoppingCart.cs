using ConsoleBuildR;
using EventSourcR;
using SqlServerSample.ShoppingCarts;
using SqlServerSample.ShoppingCarts.Commands;
using System;
using System.Threading.Tasks;

namespace SqlServerSample
{
    public class CreateTestShoppingCart : IExecutable
    {
        private readonly IRepository<ShoppingCart> _repository;

        public CreateTestShoppingCart(IRepository<ShoppingCart> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Execute(string[] args)
        {
            var shoppingCart = new ShoppingCart(Guid.NewGuid());

            shoppingCart.Handle(new CreateShoppingCart(Guid.NewGuid()));
            shoppingCart.Handle(new AddShoppingCartProduct(Guid.NewGuid(), "S.Pellegrino Sparkling Natural Mineral Water, 33.8 fl oz. (Pack of 12)", 15.53M, 4));
            shoppingCart.Handle(new AddShoppingCartProduct(Guid.NewGuid(), "Samsung 64GB 100MB/s (U3) MicroSD EVO Select Memory Card with Adapter (MB-ME64GA/AM)", 12.99M, 1));
            shoppingCart.Handle(new AddShoppingCartProduct(Guid.NewGuid(), "Samsung 970 PRO 512GB - NVMe PCIe M.2 2280 SSD (MZ-V7P512BW)", 167.99M, 1));

            await _repository.Save(shoppingCart);
        }
    }
}
