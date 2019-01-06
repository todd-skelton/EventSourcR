using System;
using System.Linq;
using System.Reflection;

namespace EventSourcR.Extensions
{
    public static class TypeMapperExtensions
    {
        public static void MapEventImplementations(this ITypeMapper mapper, Assembly assembly) => mapper.MapEventImplementations<EmptyMetadata>(assembly);

        public static void MapEventImplementations<T>(this ITypeMapper mapper, Assembly assembly) where T : IMetadata
        {
            var implementations = assembly.GetTypes().Where(t => typeof(IEvent).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

            foreach(var implementation in implementations)
            {
                var name = implementation.Name.ToKebabCase();
                mapper.MapEvent(name, implementation);
                mapper.MapMetadata<T>(name);
            }
        }

        public static void MapAggregateImplementations(this ITypeMapper mapper, Assembly assembly)
        {
            var implementations = assembly.GetTypes().Where(t => typeof(IAggregate).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

            foreach (var implementation in implementations)
            {
                mapper.MapAggregate(implementation.Name.ToKebabCase(), implementation);
            }
        }
    }
}