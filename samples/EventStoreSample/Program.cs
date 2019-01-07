using ConsoleBuildR;
using EventSourcR;
using EventSourcR.Extensions;
using EventSourcR.JsonEventSerializer;
using EventStore.ClientAPI;
using EventStoreSample.Tickets;
using EventStoreSample.Tickets.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace EventStoreSample
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
                services.AddTransient<IEventSerializer, JsonEventSerializer>();

                var mapper = new TypeMapper();
                mapper.MapEventImplementations(typeof(Program).Assembly);
                mapper.MapAggregateImplementations(typeof(Program).Assembly);

                services.AddSingleton<ITypeMapper>(mapper);

                var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
                connection.ConnectAsync().Wait();

                services.AddSingleton(connection);
                
                services.AddTransient<IEventStore, EventSourcR.EventStore.EventStore>();
                services.AddTransient<IPendingEventFactory, PendingEventFactory>();
                services.AddTransient<IRepository<Ticket>, Repository<Ticket>>();
            })
            .Execute<TestThroughput>()
            .Build();
    }

    public class TestThroughput : IExecutable
    {
        private readonly IServiceProvider _provider;

        public TestThroughput(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task Execute(string[] args)
        {
            var stopWatch = new Stopwatch();
            var rand = new Random();
            var tasks = new List<Task>();

            stopWatch.Start();

            for (var x = 0; x < 1000; x++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetService<IRepository<Ticket>>();

                        var id = Guid.NewGuid();

                        var ticket = new Ticket(id);

                        string ticketNo = rand.Next(100, 1000) + "-" + rand.Next(100, 1000);

                        var openTicket = new OpenTicket(ticketNo);

                        ticket.Handle(openTicket);

                        await repository.Save(ticket);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            stopWatch.Stop();

            var time = stopWatch.Elapsed;

            Console.WriteLine($"1000 Tickets took {time}");

            Console.WriteLine($"Throughput: {1000 / time.TotalSeconds} per second");

            return;
        }
    }
}
