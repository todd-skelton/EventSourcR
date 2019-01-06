using EventSourcR;

namespace ConsoleApp.Orders.Events
{
    public class ShippingAddressAdded : IEvent<Order>
    {
        public ShippingAddressAdded(string addressLine1, string addressLine2)
        {
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
        }

        public string AddressLine1 { get; private set; }
        public string AddressLine2 { get; private set; }

        public override string ToString() => $"Shipping address added:\n{AddressLine1}\n{AddressLine2}";
    }
}
