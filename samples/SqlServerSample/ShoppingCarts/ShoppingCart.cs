using EventSourcR;
using SqlServerSample.ShoppingCarts.Commands;
using SqlServerSample.ShoppingCarts.Events;
using System;
using System.Collections.Generic;

namespace SqlServerSample.ShoppingCarts
{
    public class ShoppingCart : AggregateBase<ShoppingCart>
    {
        private readonly IDictionary<Guid, Product> products = new Dictionary<Guid, Product>();

        public ShoppingCart(Guid id) : base(id) { }

        public Guid? CustomerId { get; private set; }

        public decimal Total { get; private set; }

        public IReadOnlyDictionary<Guid, Product> Products => products.AsReadOnly();

        public override void Handle(ICommand<ShoppingCart> command)
        {
            switch (command)
            {
                case CreateShoppingCart create:
                    RaiseEvent(new ShoppingCartCreated(create.CustomerId));
                    break;
            }
        }

        protected override void Handle(IEvent<ShoppingCart> @event)
        {
            switch (@event)
            {
                case ShoppingCartCreated created:
                    CustomerId = created.CustomerId;
                    break;
            }
        }
    }

    public class ShoppingCartState
    {
        public ShoppingCartState(Guid id, Guid? customerId, decimal total)
        {
            Id = id;
            CustomerId = customerId;
            Total = total;
        }

        public Guid Id { get; }
        public Guid? CustomerId { get; }
        public decimal Total { get; }
    }
}
