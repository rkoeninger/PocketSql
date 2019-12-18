using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PocketSql.Modeling
{
    public static class Extensions
    {
        public static Maybe<B> GetMaybe<A, B>(this ImmutableDictionary<A, B> dict, A key) =>
            dict.TryGetValue(key, out var val)
                ? Maybe.Some(val)
                : Maybe.None<B>();

        public static IEnumerable<T> ListOf<T>(T value) => new List<T> { value };

        public static T VoidNull<T>(Action f) where T : class
        {
            f();
            return null;
        }
    }
}
