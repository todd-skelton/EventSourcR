using EventSourcR;

namespace InMemorySample.BankAccounts.Events
{
    public class BankAccountOverdrawn : IEvent<BankAccount>
    {
        public override string ToString() => $"Bank account is overdrawn.";
    }
}
