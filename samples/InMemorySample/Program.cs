using ConsoleBuildR;
using EventSourcR;
using EventSourcR.Extensions;
using EventSourcR.InMemory;
using InMemorySample.BankAccounts;
using Microsoft.Extensions.DependencyInjection;

namespace InMemorySample
{
    class Program
    {
        static void Main(string[] args)
        {
            Build().Run(args);
        }

        static IConsole Build() =>
            ConsoleBuilder.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var mapper = new TypeMapper();
                var assembly = typeof(Program).Assembly;
                mapper.MapEventImplementations(assembly);
                mapper.MapAggregateImplementations(assembly);

                services.AddSingleton<ITypeMapper>(mapper);
                services.AddSingleton<IEventStore, EventStore>();
                services.AddTransient<IPendingEventFactory, PendingEventFactory>();
                services.AddTransient<IRepository<BankAccount>, Repository<BankAccount>>();
            })
            .Execute<App>()
            .Build();
    }


}
