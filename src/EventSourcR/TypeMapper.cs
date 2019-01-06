using System;
using System.Collections.Generic;

namespace EventSourcR
{
    public class TypeMapper : ITypeMapper
    {
        private readonly IDictionary<string, Type> _eventTypeMap = new Dictionary<string, Type>();
        private readonly IDictionary<Type, string> _eventNameMap = new Dictionary<Type, string>();
        private readonly IDictionary<string, Type> _aggregateTypeMap = new Dictionary<string, Type>();
        private readonly IDictionary<Type, string> _aggregateNameMap = new Dictionary<Type, string>();
        private readonly IDictionary<string, Type> _metaDataMap = new Dictionary<string, Type>();

        public Type GetEventType(string name)
        {
            _eventTypeMap.TryGetValue(name, out var type);

            if (type is null) throw new ArgumentException($"No suitable type is mapped for the name: {name}.");

            return type;
        }

        public string GetEventName<T>() => GetEventName(typeof(T));

        public string GetEventName(Type type)
        {
            _eventNameMap.TryGetValue(type, out var name);

            if (name is null) throw new ArgumentException($"No suitable name is mapped for the type: {type.Name}.");

            return name;
        }

        public void MapEvent<T>(string name) => MapEvent(name, typeof(T));

        public void MapEvent(string name, Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (_eventTypeMap.ContainsKey(name) || _eventNameMap.ContainsKey(type)) throw new ArgumentException($"Unable to map name: {name} to type: {type.Name}.");

            _eventTypeMap[name] = type;
            _eventNameMap[type] = name;
        }

        public Type GetAggregateType(string name)
        {
            _aggregateTypeMap.TryGetValue(name, out var type);

            if (type is null) throw new ArgumentException($"No suitable type is mapped for the name: {name}.");

            return type;
        }

        public string GetAggregateName<T>() => GetAggregateName(typeof(T));

        public string GetAggregateName(Type type)
        {
            _aggregateNameMap.TryGetValue(type, out var name);

            if (name is null) throw new ArgumentException($"No suitable name is mapped for the type: {type.Name}.");

            return name;
        }

        public void MapAggregate<T>(string name) => MapAggregate(name, typeof(T));

        public void MapAggregate(string name, Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (_aggregateTypeMap.ContainsKey(name) || _aggregateNameMap.ContainsKey(type)) throw new ArgumentException($"Unable to map name: {name} to type: {type.Name}.");

            _aggregateTypeMap[name] = type;
            _aggregateNameMap[type] = name;
        }

        public Type GetMetadataType(string name)
        {
            _metaDataMap.TryGetValue(name, out var type);

            if (type is null) throw new ArgumentException($"No suitable metadata type is mapped to the event name: {name}");

            return type;
        }

        public void MapMetadata<T>(string name) => MapMetadata(name, typeof(T));

        public void MapMetadata(string name, Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (!_eventTypeMap.ContainsKey(name)) throw new ArgumentException($"Event name: {name} must be mapped before metadata type.");
            if (_metaDataMap.ContainsKey(name)) throw new ArgumentException($"Event name: {name} is already mapped to metadata type: {_metaDataMap[name].Name}.");

            _metaDataMap[name] = type;
        }
    }
}
