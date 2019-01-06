using EventSourcR;
using System;

namespace SqlServerSample.ShoppingCarts.Events
{
    public class ShoppingCartProductRemoved : IEvent<ShoppingCart>
    {
        public ShoppingCartProductRemoved(Guid productId)
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
