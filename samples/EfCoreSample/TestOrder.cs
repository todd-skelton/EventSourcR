using EfCoreSample.Orders;
using EfCoreSample.Orders.Commands;
using ConsoleBuildR;
using EventSourcR;
using System;
using System.Threading.Tasks;

namespace EfCoreSample
{
    public class TestOrder : IExecutable
    {
        private readonly IRepository<Order> _repository;

        public TestOrder(IRepository<Order> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Execute(string[] args)
        {
            var id = Guid.NewGuid();
            var order = new Order(id);

            var createOrder = new CreateOrder("168-984");

            order.Execute(createOrder);

            await _repository.Save(order);

            order = await _repository.Load(new Order(id));

            var addShipping = new AddShippingAddress("12525 W Binter St", "Wichita, KS, 67235");

            order.Execute(addShipping);

            await _repository.Save(order);
        }
    }
}
