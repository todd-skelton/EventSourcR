namespace ConsoleApp.Orders
{
    public class OrderItem
    {
        public OrderItem(int line, string productName)
        {
            Line = line;
            ProductName = productName;
        }

        public int Line { get; private set; }
        public string ProductName { get; private set; }
    }
}
