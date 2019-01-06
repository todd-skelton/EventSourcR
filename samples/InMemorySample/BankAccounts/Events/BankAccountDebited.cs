using EventSourcR;

namespace InMemorySample.BankAccounts.Events
{
    public class BankAccountDebited : IEvent<BankAccount>
    {
        public BankAccountDebited(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; }

        public override string ToString() => $"Bank account was debited {Amount.ToString("c")}";
    }
}
