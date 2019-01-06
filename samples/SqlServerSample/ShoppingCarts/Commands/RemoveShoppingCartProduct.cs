using EventSourcR;
using System;

namespace SqlServerSample.ShoppingCarts.Commands
{
    public class RemoveShoppingCartProduct : ICommand<ShoppingCart>
    {
        public RemoveShoppingCartProduct(Guid productId)
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
