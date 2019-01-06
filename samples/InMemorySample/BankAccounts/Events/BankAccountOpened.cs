using EventSourcR;

namespace InMemorySample.BankAccounts.Events
{
    public class BankAccountOpened : IEvent<BankAccount>
    {
        public override string ToString() => $"Bank account was opened.";
    }
}
