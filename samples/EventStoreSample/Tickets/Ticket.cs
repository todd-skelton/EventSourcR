using EventSourcR;
using EventStoreSample.Tickets.Commands;
using EventStoreSample.Tickets.Events;
using System;

namespace EventStoreSample.Tickets
{
    public class Ticket : AggregateBase<Ticket>
    {
        public Ticket(Guid id) : base(id)
        {
        }

        public string TicketNumber { get; private set; }
        public TicketState State { get; private set; }

        public override void Issue<TCommand>(TCommand command)
        {
            switch (command)
            {
                case OpenTicket openTicket:
                    RaiseEvent(new TicketOpened(openTicket.TicketNumber));
                    break;
                case CloseTicket closeTicket:
                    RaiseEvent(new TicketClosed());
                    break;
            }
        }

        protected override void Handle<TEvent>(TEvent @event)
        {
            switch (@event)
            {
                case TicketOpened ticketOpened:
                    TicketNumber = ticketOpened.TicketNumber;
                    State = TicketState.Open;
                    break;
                case TicketClosed ticketClosed:
                    State = TicketState.Closed;
                    break;
            }
        }
    }
}
