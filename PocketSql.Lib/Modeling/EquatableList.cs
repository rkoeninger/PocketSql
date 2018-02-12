using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public class EquatableList : IEquatable<EquatableList>
    {
        public static EquatableList Of(IEnumerable<(string, object)> seq)
        {
            var list = new EquatableList();

            foreach (var x in seq)
            {
                list.Elements.Add(x);
            }

            return list;
        }

        public IList<(string, object)> Elements { get; } = new List<(string, object)>();

        public override bool Equals(object obj) =>
            obj is EquatableList && Equals((EquatableList)obj);

        public override int GetHashCode() =>
            Elements.Aggregate(1, (hash, obj) => hash + obj.GetHashCode() * 357);

        public bool Equals(EquatableList other) => Elements.SequenceEqual(other.Elements);
    }
}
