using EventSourcR;
using System;

namespace EventStoreSample.Tickets.Commands
{
    public class OpenTicket : ICommand<Ticket>
    {
        public OpenTicket(string ticketNumber)
        {
            TicketNumber = ticketNumber ?? throw new ArgumentNullException(nameof(ticketNumber));
        }

        public string TicketNumber { get; }
    }
}
