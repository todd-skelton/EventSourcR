using EfCoreSample.Orders;
using EfCoreSample.Orders.Commands;
using ConsoleBuildR;
using EventSourcR;
using System;
using System.Threading.Tasks;

namespace EfCoreSample
{
    public class CreateOrders : IExecutable
    {
        private readonly IRepository<Order> _repository;

        public CreateOrders(IRepository<Order> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task Execute(string[] args)
        {
            var rand = new Random();

            for (var x = 0; x < 10; x++)
            {
                var order = new Order(Guid.NewGuid());

                string orderNo = rand.Next(100, 1000) + "-" + rand.Next(100, 1000);

                var createOrder = new CreateOrder(orderNo);

                order.Execute(createOrder);

                _repository.Save(order);
            }
            return Task.CompletedTask;
        }
    }
}
