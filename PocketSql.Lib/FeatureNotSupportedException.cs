using System;

namespace PocketSql
{
    public class FeatureNotSupportedException : Exception
    {
        public static FeatureNotSupportedException Value(object value, object context) =>
            new FeatureNotSupportedException($"{value} is not a supported value of {context}");

        public static FeatureNotSupportedException Value<T>(T val) =>
            Value(val, typeof(T).Name);

        public static FeatureNotSupportedException Subtype<T>(T obj) =>
            new FeatureNotSupportedException($"{obj.GetType().Name} is not a supported type of {typeof(T).Name}");

        public FeatureNotSupportedException(string message)
            : base(message) { }
    }
}
