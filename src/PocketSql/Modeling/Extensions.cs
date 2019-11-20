using System.Collections.Immutable;

namespace PocketSql.Modeling
{
    public static class Extensions
    {
        public static Maybe<B> GetMaybe<A, B>(this ImmutableDictionary<A, B> dict, A key) =>
            dict.TryGetValue(key, out var val)
                ? Maybe.Some(val)
                : Maybe.None<B>();
    }
}
