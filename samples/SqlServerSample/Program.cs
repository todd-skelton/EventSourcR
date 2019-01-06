using ConsoleBuildR;
using EventSourcR;
using EventSourcR.Extensions;
using EventSourcR.JsonEventSerializer;
using EventSourcR.SqlServer;
using EventSourcR.SqlServer.Reactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlServerSample.ShoppingCarts;
using System.Threading.Tasks;

namespace SqlServerSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Build().RunAsync(args);
        }

        static IConsole Build() =>
            ConsoleBuilder.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IEventSerializer, JsonEventSerializer>();

                var mapper = new TypeMapper();
                mapper.MapEventImplementations(typeof(Program).Assembly);
                mapper.MapAggregateImplementations(typeof(Program).Assembly);

                services.AddSingleton(new EventStoreOptions() { ConnectionString = context.Configuration.GetConnectionString("EventStore") });
                services.AddSingleton<ITypeMapper>(mapper);
                services.AddTransient<IEventStore, EventStore>();
                services.AddTransient<IPendingEventFactory, PendingEventFactory>();
                services.AddTransient<IRepository<ShoppingCart>, Repository<ShoppingCart>>();
                services.AddSingleton<IEventReactor, EventReactor>();
            })
            .Execute<TestWriteThroughput>()
            .Build();
    }
}
