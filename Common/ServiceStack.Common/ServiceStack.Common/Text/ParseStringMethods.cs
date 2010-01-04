using System;
using System.Collections.Generic;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Utils;

namespace ServiceStack.Common.Text
{
	public static class ParseStringMethods
	{
		public static Func<string, object> GetParseMethod(Type type)
		{
			type = Nullable.GetUnderlyingType(type) ?? type;

			if (type.IsEnum)
			{
				return value => ParseEnum(type, value);
			}

			if (type == typeof(string))
				return ParseString;

			if (type.IsEnum)
				return value => ParseEnum(type, value);

			if (type.IsArray)
			{
				return ParseStringArrayMethods.GetParseMethod(type);
			}

			var builtInMethod = ParseStringBuiltinMethods.GetParseMethod(type);
			if (builtInMethod != null)
				return builtInMethod;

			if (type.IsGenericType())
			{
				var listInterfaces = type.FindInterfaces(
					(t, critera) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>), null);
				if (listInterfaces.Length > 0)
					return ParseStringListMethods.GetParseMethod(type);

				var mapInterfaces = type.FindInterfaces(
					(t, critera) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>), null);
				if (mapInterfaces.Length > 0)
					return ParseStringDictionaryMethods.GetParseMethod(type);

				var collectionInterfaces = type.FindInterfaces(
					(t, critera) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>), null);

				if (collectionInterfaces.Length > 0)
					return ParseStringCollectionMethods.GetParseMethod(type);
			}

			var staticParseMethod = ParseStringStaticParseMethod.GetParseMethod(type);
			if (staticParseMethod != null)
				return staticParseMethod;

			var typeConstructor = ParseStringTypeMethod.GetParseMethod(type);
			if (typeConstructor != null)
				return typeConstructor;

			var stringConstructor = ParseStringTypeConstructor.GetParseMethod(type);
			if (stringConstructor != null) return stringConstructor;

			return null;
		}
		
		public static object NullValueType(Type type)
		{
			return ReflectionUtils.GetDefaultValue(type);
		}

		public static string ParseString(string value)
		{
			return value.FromSafeString();
		}

		public static object ParseEnum(Type type, string value)
		{
			return Enum.Parse(type, value);
		}

	}
}