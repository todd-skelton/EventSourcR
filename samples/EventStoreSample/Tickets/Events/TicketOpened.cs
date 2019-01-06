using EventSourcR;
using System;

namespace EventStoreSample.Tickets.Events
{
    public class TicketOpened : IEvent<Ticket>
    {
        public TicketOpened(string ticketNumber)
        {
            TicketNumber = ticketNumber ?? throw new ArgumentNullException(nameof(ticketNumber));
        }

        public string TicketNumber { get; }
    }
}
