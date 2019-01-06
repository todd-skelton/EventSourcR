using EventSourcR;
using System;

namespace ConsoleApp.Orders.Commands
{
    public class CreateOrder : ICommand<Order>
    {
        public CreateOrder(string orderNumber)
        {
            OrderNumber = orderNumber ?? throw new ArgumentNullException(nameof(orderNumber));
        }

        public string OrderNumber { get; private set; }
    }
}
