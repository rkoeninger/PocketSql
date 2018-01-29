using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql
{
    public class EquatableList : IEquatable<EquatableList>
    {
        public static EquatableList Of(object[] array)
        {
            var list = new EquatableList();

            foreach (var x in array)
            {
                list.Elements.Add(x);
            }

            return list;
        }

        public IList<object> Elements { get; } = new List<object>();

        public override bool Equals(object obj) =>
            obj is EquatableList && Equals((EquatableList)obj);

        public override int GetHashCode() =>
            Elements.Aggregate(1, (hash, obj) => hash + (obj?.GetHashCode() ?? 1) * 357);

        public bool Equals(EquatableList other) =>
            Elements.Count == other.Elements.Count
            && Enumerable.Range(0, Elements.Count)
                .All(i => Elements[i]?.Equals(other.Elements[i]) ?? other.Elements[i] == null);
    }
}
