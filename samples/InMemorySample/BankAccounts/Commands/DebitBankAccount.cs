using EventSourcR;
using System;

namespace InMemorySample.BankAccounts.Commands
{
    public class DebitBankAccount : ICommand<BankAccount>
    {
        public DebitBankAccount(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Debit amount can't be less than 0.");
            Amount = amount;
        }

        public decimal Amount { get; }
    }
}
