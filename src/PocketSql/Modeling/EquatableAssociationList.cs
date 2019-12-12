using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public class EquatableAssociationList : IEquatable<EquatableAssociationList>
    {
        public static EquatableAssociationList Of(IEnumerable<(string, object)> seq)
        {
            var list = new EquatableAssociationList();

            foreach (var x in seq)
            {
                list.Elements.Add(x);
            }

            return list;
        }

        public IList<(string, object)> Elements { get; } = new List<(string, object)>();

        public override bool Equals(object obj) =>
            obj is EquatableAssociationList list && Equals(list);

        public override int GetHashCode() =>
            Elements.Aggregate(1, (hash, obj) => hash + obj.GetHashCode() * 357);

        public bool Equals(EquatableAssociationList other) =>
            other?.Elements.SequenceEqual(Elements) ?? false;
    }
}
