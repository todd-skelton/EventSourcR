using System;

namespace EventSourcR
{
    /// <summary>
    /// Gets a type using a configured name.
    /// </summary>
    public interface ITypeMapper
    {
        Type GetEventType(string name);
        string GetEventName<T>();
        string GetEventName(Type type);
        void MapEvent<T>(string name);
        void MapEvent(string name, Type type);

        Type GetAggregateType(string name);
        string GetAggregateName<T>();
        string GetAggregateName(Type type);
        void MapAggregate<T>(string name);
        void MapAggregate(string name, Type type);

        Type GetMetadataType(string name);
        void MapMetadata<T>(string name);
        void MapMetadata(string name, Type type);
    }
}
