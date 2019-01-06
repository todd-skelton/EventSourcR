using EventSourcR;
using System;

namespace SqlServerSample.ShoppingCarts.Commands
{
    public class CreateShoppingCart : ICommand<ShoppingCart>
    {
        public CreateShoppingCart(Guid? customerId)
        {
            CustomerId = customerId;
        }

        public Guid? CustomerId { get; }
    }
}
