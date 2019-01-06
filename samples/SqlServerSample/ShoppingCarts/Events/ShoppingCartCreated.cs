using EventSourcR;
using System;

namespace SqlServerSample.ShoppingCarts.Events
{
    public class ShoppingCartCreated : IEvent<ShoppingCart>
    {
        public ShoppingCartCreated(Guid? customerId)
        {
            CustomerId = customerId;
        }

        public Guid? CustomerId { get; }
    }
}
