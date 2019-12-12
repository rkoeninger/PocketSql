using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public static class EquatableArray
    {
        public static EquatableArray<T> Of<T>(IEnumerable<T> seq) where T : IEquatable<T>
        {
            var list = new EquatableArray<T>();

            foreach (var x in seq)
            {
                list.Elements.Add(x);
            }

            return list;
        }
    }

    public class EquatableArray<T> : IEquatable<EquatableArray<T>> where T : IEquatable<T>
    {
        public IList<T> Elements { get; } = new List<T>();

        public override bool Equals(object obj) =>
            obj is EquatableArray<T> array && Equals(array);

        public override int GetHashCode() =>
            Elements.Aggregate(1, (hash, obj) => hash + obj.GetHashCode() * 357);

        public bool Equals(EquatableArray<T> other) =>
            other?.Elements.SequenceEqual(Elements) ?? false;
    }
}
