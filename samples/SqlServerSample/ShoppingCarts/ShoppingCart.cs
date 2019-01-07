using EventSourcR;
using SqlServerSample.ShoppingCarts.Commands;
using SqlServerSample.ShoppingCarts.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServerSample.ShoppingCarts
{
    public class ShoppingCart : AggregateBase<ShoppingCart>
    {
        private readonly IDictionary<Guid, Product> products = new Dictionary<Guid, Product>();

        public ShoppingCart(Guid id) : base(id) { }

        // customer ID
        public Guid? CustomerId { get; private set; }

        // cart total
        public decimal Total { get; private set; }

        //list of products
        public IReadOnlyDictionary<Guid, Product> Products => products.AsReadOnly();

        // validate the command and raise event if it's valid
        public override void Handle(ICommand<ShoppingCart> command)
        {
            switch (command)
            {
                case CreateShoppingCart create:
                    RaiseEvent(new ShoppingCartCreated(create.CustomerId));
                    break;
                case AddShoppingCartProduct addProduct:
                    RaiseEvent(new ShoppingCartProductAdded(addProduct.ProductId, addProduct.ProductName, addProduct.ProductPrice, addProduct.Quantity));
                    break;
                case RemoveShoppingCartProduct removeProduct:
                    // make sure product exists.
                    if (products.ContainsKey(removeProduct.ProductId))
                    {
                        RaiseEvent(new ShoppingCartProductRemoved(removeProduct.ProductId));
                    }
                    break;
            }
        }

        // update the state of the aggregate
        protected override void Handle(IEvent<ShoppingCart> @event)
        {
            switch (@event)
            {
                case ShoppingCartCreated created:
                    CustomerId = created.CustomerId;
                    break;
                case ShoppingCartProductAdded productAdded:
                    if (products.ContainsKey(productAdded.ProductId))
                    {
                        // update quantity of current product
                        products[productAdded.ProductId] = new Product(productAdded.ProductId, productAdded.ProductName, productAdded.ProductPrice, products[productAdded.ProductId].Quantity + productAdded.Quantity);
                    }
                    else
                    {
                        // add new product
                        products[productAdded.ProductId] = new Product(productAdded.ProductId, productAdded.ProductName, productAdded.ProductPrice, productAdded.Quantity);
                    }
                    Total = products.Values.Sum(e => e.Price * e.Quantity);
                    break;
                case ShoppingCartProductRemoved productRemoved:
                    products.Remove(productRemoved.ProductId);
                    Total = products.Values.Sum(e => e.Price * e.Quantity);
                    break;
            }
        }
    }
}
