using System;

namespace SqlServerSample.ShoppingCarts
{
    public class Product
    {
        public Product(Guid id, string name, decimal price, string quantity)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public Guid Id { get; }
        public string Name { get; }
        public decimal Price { get; }
        public string Quantity { get; }
    }
}
