using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ServiceStack.Common
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, List<string>> TypePropertyNamesMap = new Dictionary<Type, List<string>>();

        public static List<string> GetPropertyNames(this Type type)
        {
            lock (TypePropertyNamesMap)
            {
                List<string> propertyNames;
                if (!TypePropertyNamesMap.TryGetValue(type, out propertyNames))
                {
#if NETFX_CORE
                    propertyNames = Extensions.EnumerableExtensions.ConvertAll(type.GetRuntimeProperties(), x => x.Name);
#else
                    propertyNames = Extensions.EnumerableExtensions.ConvertAll(type.GetProperties(), x => x.Name);
#endif
                    TypePropertyNamesMap[type] = propertyNames;
                }
                return propertyNames;
            }
        }

        public static List<T> ToAttributes<T>(this Type type) where T : Attribute
        {
#if NETFX_CORE
            return type.GetTypeInfo().GetCustomAttributes<T>(true).ToList();
#else
            return type.GetCustomAttributes(typeof(T), true).SafeConvertAll(x => (T)x);
#endif
        }

#if !SILVERLIGHT
        public static string GetAssemblyPath(this Type source)
        {
            var assemblyUri =
                new Uri(source.Assembly.EscapedCodeBase);

            return assemblyUri.LocalPath;
        }
#endif
    }
}
