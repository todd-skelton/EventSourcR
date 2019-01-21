using EventSourcR;
using InMemorySample.BankAccounts.Commands;
using InMemorySample.BankAccounts.Events;
using System;

namespace InMemorySample.BankAccounts
{
    public class BankAccount : AggregateBase<BankAccount>
    {
        public BankAccount(Guid id) : base(id) { }

        public decimal Balance { get; private set; }
        public bool IsInGoodStanding { get; private set; }

        public override void Issue<TCommand>(TCommand command)
        {
            switch (command)
            {
                case OpenBankAccount openBankAccount:
                    RaiseEvent(new BankAccountOpened());
                    break;
                case DebitBankAccount debitBankAccount:
                    RaiseEvent(new BankAccountDebited(debitBankAccount.Amount));
                    if (IsInGoodStanding && Balance < 0)
                        RaiseEvent(new BankAccountOverdrawn());
                    break;
                case CreditBankAccount creditBankAccount:
                    RaiseEvent(new BankAccountCredited(creditBankAccount.Amount));
                    if (!IsInGoodStanding && Balance > 0)
                        RaiseEvent(new BankAccountNoLongerOverdrawn());
                    break;
            }
        }

        protected override void Handle<TEvent>(TEvent @event)
        {
            switch (@event)
            {
                case BankAccountOpened bankAccountOpened:
                    Balance = 0;
                    IsInGoodStanding = true;
                    break;
                case BankAccountCredited bankAccountCredited:
                    Balance += bankAccountCredited.Amount;
                    break;
                case BankAccountDebited bankAccountDebited:
                    Balance -= bankAccountDebited.Amount;
                    break;
                case BankAccountOverdrawn bankAccountOverdrawn:
                    IsInGoodStanding = false;
                    break;
                case BankAccountNoLongerOverdrawn bankAccountNoLongerOverdrawn:
                    IsInGoodStanding = true;
                    break;
            }
        }
    }
}
