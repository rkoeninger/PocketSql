using System;

namespace PocketSql.Modeling
{
    public static class Types
    {
        public static DateTime AsDateTime(this object x)
        {
            if (x is DateTime) return (DateTime)x;
            if (x is string s) return DateTime.Parse(s);

            throw new InvalidCastException($"Type {x?.GetType().Name} cannot be converted to datetime");
        }

        public static int AsInt(this object x)
        {
            if (x is int) return (int)x;

            throw new InvalidCastException($"Type {x?.GetType().Name} cannot be converted to int");
        }

        public static string AsString(this object x)
        {
            if (x is string) return (string)x;

            throw new InvalidCastException($"Type {x?.GetType().Name} cannot be converted to varchar");
        }

        public static T As<T>(this object x)
        {
            if (typeof(T) == typeof(object)) return (T)x;
            if (typeof(T) == typeof(DateTime)) return (T)(object)x.AsDateTime();
            if (typeof(T) == typeof(int)) return (T)(object)x.AsInt();
            if (typeof(T) == typeof(string)) return (T)(object)x.AsString();

            throw new InvalidCastException($"Type {x?.GetType().Name} cannot be converted to {typeof(T).Name}");
        }
    }
}
