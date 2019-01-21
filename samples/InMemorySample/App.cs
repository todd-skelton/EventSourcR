using ConsoleBuildR;
using EventSourcR;
using InMemorySample.BankAccounts;
using InMemorySample.BankAccounts.Commands;
using System;
using System.Threading.Tasks;

namespace InMemorySample
{
    public class App : IExecutable
    {
        private readonly IEventStore _eventStore;
        private readonly IRepository<BankAccount> _repository;

        public App(IEventStore eventStore, IRepository<BankAccount> repository)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Execute(string[] args)
        {
            var account = new BankAccount(Guid.NewGuid());

            var openAccount = new OpenBankAccount();

            account.Issue(openAccount);

            await _repository.Save(account);

            var rand = new Random();

            for(var x = 0; x < 10; x++)
            {
                var isCredit = rand.NextDouble() >= 0.5;
                var amount = rand.Next(0, 100);

                if (isCredit)
                    account.Issue(new CreditBankAccount(amount));
                else
                    account.Issue(new DebitBankAccount(amount));

                foreach(var @event in account.PendingEvents)
                {
                    Console.WriteLine(@event);
                }
                Console.WriteLine($"Current balance is {account.Balance.ToString("c")}");
                Console.WriteLine("");

                await _repository.Save(account);

                await Task.Delay(1000);
            }

            Console.WriteLine("Reading back events directly from event store:");
            Console.WriteLine("");
            Console.WriteLine("No\tType\tMessage");
            foreach (var @event in await _eventStore.GetEvents(1, 100))
            {
                Console.WriteLine($"{@event.EventNumber}\t{@event.EventType}\t{@event.Data}");
            }
        }
    }
}
