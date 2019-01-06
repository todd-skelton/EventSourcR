using EventSourcR;

namespace InMemorySample.BankAccounts.Events
{
    public class BankAccountCredited : IEvent<BankAccount>
    {
        public BankAccountCredited(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; }

        public override string ToString() => $"Bank account was credited {Amount.ToString("c")}";
    }
}
