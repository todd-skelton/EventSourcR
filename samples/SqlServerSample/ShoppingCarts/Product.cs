using System;

namespace SqlServerSample.ShoppingCarts
{
    public class Product
    {
        public Product(Guid productId, string name, decimal price, int quantity)
        {
            ProductId = productId;
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public Guid ProductId { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; }
    }
}
