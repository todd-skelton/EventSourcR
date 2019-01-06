using EventSourcR;

namespace InMemorySample.BankAccounts.Events
{
    public class BankAccountNoLongerOverdrawn : IEvent<BankAccount>
    {
        public override string ToString() => $"Bank account is no longer overdrawn.";
    }
}
