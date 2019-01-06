using EventSourcR;
using System;

namespace ConsoleApp.Orders.Events
{
    public class OrderCreated : IEvent<Order>
    {
        public OrderCreated(string orderNumber)
        {
            OrderNumber = orderNumber ?? throw new ArgumentNullException(nameof(orderNumber));
        }

        public string OrderNumber { get; }

        public override string ToString() => $"Order created with number: {OrderNumber}";
    }
}
