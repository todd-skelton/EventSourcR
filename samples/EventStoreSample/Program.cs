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
        private readonly IRepository<Ticket> _repository;

        public TestThroughput(IRepository<Ticket> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Execute(string[] args)
        {
            var stopWatch = new Stopwatch();
            var rand = new Random();
            var tasks = new List<Task>();

            stopWatch.Start();

            for (var x = 0; x < 10000; x++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var id = Guid.NewGuid();

                    var ticket = new Ticket(id);

                    string ticketNo = rand.Next(100, 1000) + "-" + rand.Next(100, 1000);

                    var openTicket = new OpenTicket(ticketNo);

                    ticket.Execute(openTicket);

                    return _repository.Save(ticket);
                }));
            }

            await Task.WhenAll(tasks);

            stopWatch.Stop();

            var time = stopWatch.Elapsed;

            Console.WriteLine($"10000 Tickets took {time}");

            Console.WriteLine($"Throughput: {10000 / time.TotalSeconds} per second");

            return;
        }
    }
}
