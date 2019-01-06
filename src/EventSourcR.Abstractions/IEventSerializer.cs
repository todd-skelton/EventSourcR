using System;

namespace EventSourcR
{
    public interface IEventSerializer
    {
        string Serialize(object @object);
        T Deserialize<T>(string serializedObject);
        object Deserialize(string serializedObject, Type type);
    }
}
