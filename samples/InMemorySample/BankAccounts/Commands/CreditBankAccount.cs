using EventSourcR;
using System;

namespace InMemorySample.BankAccounts.Commands
{
    public class CreditBankAccount : ICommand<BankAccount>
    {
        public CreditBankAccount(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Credit amount can't be less than 0.");
            Amount = amount;
        }

        public decimal Amount { get; }
    }
}
