using System;
using System.Collections.Generic;

namespace PocketSql.Modeling
{
    public static class Types
    {
        private static readonly Dictionary<(Type, Type), Func<object, object>> casters =
            new Dictionary<(Type, Type), Func<object, object>>();

        private static void Setup<A, B>(Func<A, B> f) => casters.Add((typeof(A), typeof(B)), x => f((A) x));

        static Types()
        {
            Setup<DateTime, string>(x => x.ToString());
            Setup<int, string>(x => x.ToString());
            Setup<long, string>(x => x.ToString());
            Setup<short, string>(x => x.ToString());
            Setup<string, DateTime>(DateTime.Parse);
        }

        public static T As<T>(this object x)
        {
            var type = typeof(T);

            if (x == null)
            {
                return type.IsClass
                    ? default(T)
                    : throw new InvalidCastException($"Type {type.Name} cannot be null");
            }

            return
                type.IsAssignableFrom(x.GetType()) ? (T)x :
                casters.TryGetValue((x.GetType(), type), out var f) ? (T)f(x) :
                throw new InvalidCastException($"Type {x?.GetType().Name} cannot be converted to {typeof(T).Name}");
        }
    }
}
