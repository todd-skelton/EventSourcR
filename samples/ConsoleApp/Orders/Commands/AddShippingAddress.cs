using EventSourcR;

namespace ConsoleApp.Orders.Commands
{
    public class AddShippingAddress : ICommand<Order>
    {
        public AddShippingAddress(string addressLine1, string addressLine2)
        {
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
        }

        public string AddressLine1 { get; private set; }
        public string AddressLine2 { get; private set; }
    }
}
