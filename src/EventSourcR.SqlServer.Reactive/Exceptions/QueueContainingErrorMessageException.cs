using EventSourcR.SqlServer.Reactive.Base.Exceptions;
using EventSourcR.SqlServer.Reactive.Messages;

namespace EventSourcR.SqlServer.Reactive.Exceptions
{
    public class QueueContainingErrorMessageException : TableDependencyException
    {
        protected internal QueueContainingErrorMessageException()
            : base($"Queue containig a '{SqlMessageTypes.ErrorType}' message.")
        { }
    }
}