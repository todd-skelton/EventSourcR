using ConsoleBuildR;
using EventSourcR;
using EventSourcR.Extensions;
using EventSourcR.JsonEventSerializer;
using EventSourcR.SqlServer;
using EventSourcR.SqlServer.Reactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlServerSample.ShoppingCarts;
using System;
using System.Threading.Tasks;

namespace SqlServerReaderSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await Build().Run(args);
            }
            catch(Exception ex)
            {
                Console.Write(ex);
            }
        }

        static IConsole Build() =>
            ConsoleBuilder.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IEventSerializer, JsonEventSerializer>();

                var mapper = new TypeMapper();
                mapper.MapEventImplementations(typeof(ShoppingCart).Assembly);
                mapper.MapAggregateImplementations(typeof(ShoppingCart).Assembly);

                services.AddSingleton(new EventStoreOptions() { ConnectionString = context.Configuration.GetConnectionString("EventStore") });
                services.AddSingleton<ITypeMapper>(mapper);
                services.AddTransient<IEventStore, EventStore>();
                services.AddSingleton<IEventReactor, EventReactor>();
            })
            .Execute<LogLiveEventsReactively>()
            //.Execute<LogLiveEventsByPolling>()
            .Build();
    }
}
