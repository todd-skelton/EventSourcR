using EfCoreSample.Orders.Commands;
using EfCoreSample.Orders.Events;
using EventSourcR;
using System;

namespace EfCoreSample.Orders
{
    public class Order : AggregateBase<Order>
    {
        public Order(Guid id) : base(id) { }

        public string OrderNumber { get; private set; }
        public string ShippingAddressLine1 { get; private set; }
        public string ShippingAddressLine2 { get; private set; }

        public override void Handle(ICommand<Order> command)
        {
            switch (command)
            {
                case CreateOrder createOrder:
                    RaiseEvent(new OrderCreated(createOrder.OrderNumber));
                    break;
                case AddShippingAddress addShippingAddress:
                    RaiseEvent(new ShippingAddressAdded(addShippingAddress.AddressLine1, addShippingAddress.AddressLine2));
                    break;
                default:
                    throw new InvalidOperationException($"{command.GetType().Name} doesn't have a handler associated with it.");
            }
        }

        protected override void Handle(IEvent<Order> @event)
        {
            switch (@event)
            {
                case OrderCreated orderCreated:
                    OrderNumber = orderCreated.OrderNumber;
                    break;
                case ShippingAddressAdded shippingAddressAdded:
                    ShippingAddressLine1 = shippingAddressAdded.AddressLine1;
                    ShippingAddressLine2 = shippingAddressAdded.AddressLine2;
                    break;
                default:
                    throw new InvalidOperationException($"{@event.GetType().Name} doesn't have a handler associated with it.");
            };
        }
    }
}
