using System;

namespace EventSourcR
{
    public interface IAggregate
    {
        Guid Id { get; }
        long Version { get; }
    }
}
