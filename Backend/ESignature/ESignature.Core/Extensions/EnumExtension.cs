using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ESignature.Core.Extensions
{
    public static class EnumExtension
    {
        public static bool IsTypeNullable(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type GetBaseType(Type type)
        {
            return IsTypeNullable(type) ? type.GetGenericArguments()[0] : type;
        }

        public static bool In<T>(this T val, params T[] values) where T : IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return values.Contains(val);
        }

        public static string ToDescription(this Enum val)
        {
            return val.GetType()
                    .GetMember(val.ToString())
                    .First()
                    .GetCustomAttribute<DescriptionAttribute>()?
                    .Description ?? string.Empty;
        }
    }
}