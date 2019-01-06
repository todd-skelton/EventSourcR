using EventSourcR;
using System;

namespace SqlServerSample.ShoppingCarts.Events
{
    public class ShoppingCartProductAdded : IEvent<ShoppingCart>
    {
        public ShoppingCartProductAdded(Guid productId, string productName, decimal productPrice, int quantity)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            Quantity = quantity;
        }

        public Guid ProductId { get; }
        public string ProductName { get; }
        public decimal ProductPrice { get; }
        public int Quantity { get; }
    }
}
