using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace EventSourcR.JsonEventSerializer
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonEventSerializer() : this(null)
        {
        }

        public JsonEventSerializer(JsonSerializerSettings settings)
        {
            _settings = settings ?? new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
            };
        }

        public T Deserialize<T>(string serializedObject) => JsonConvert.DeserializeObject<T>(serializedObject, _settings);

        public object Deserialize(string serializedObject, Type type) => JsonConvert.DeserializeObject(serializedObject, type, _settings);

        public string Serialize(object @object) => JsonConvert.SerializeObject(@object, _settings);
    }
}
