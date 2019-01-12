using EfCoreSample.Orders;
using ConsoleBuildR;
using EventSourcR;
using EventSourcR.EntityFrameworkCore;
using EventSourcR.Extensions;
using EventSourcR.JsonEventSerializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EfCoreSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Build().Run(args);
        }

        static IConsole Build() =>
            ConsoleBuilder.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IEventSerializer, JsonEventSerializer>();

                var mapper = new TypeMapper();
                mapper.MapEventImplementations(typeof(Program).Assembly);
                mapper.MapAggregateImplementations(typeof(Program).Assembly);

                services.AddSingleton<ITypeMapper>(mapper);
                services.AddDbContext<IEventStore, EventStore>(options => options.UseSqlServer(context.Configuration.GetConnectionString("EventStore")));
                services.AddTransient<IPendingEventFactory, PendingEventFactory>();
                services.AddTransient<IRepository<Order>, Repository<Order>>();
            })
            //.Execute<CreateOrders>()
            .Execute<TestOrder>()
            .Execute<TestThroughput>()
            //.Execute<LogEvents>()
            .Build();
    }
}
