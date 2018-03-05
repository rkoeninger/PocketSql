using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public class Row
    {
        public IList<Column> Columns { get; set; } = new List<Column>();
        public IList<object> Values { get; set; } = new List<object>();
        public bool IsNull(int i) => Values[i] == null || Values[i] == DBNull.Value;

        public Column GetColumn(int ordinal) => Columns[ordinal];

        public Column GetColumn(string name) => GetColumn(GetColumnOrdinal(name));

        public int GetColumnOrdinal(string name)
        {
            for (var i = 0; i < Columns.Count; ++i)
                if (name != null && name.Similar(Columns[i].Name.LastOrDefault()))
                    return i;

            throw new Exception($"Column \"{name}\" does not exist in row");
        }

        // TODO: expand name with default database/schema/match tables in scope
        public int GetColumnOrdinal(string[] name, Scope scope)
        {
            if (name.Length == 0) throw new Exception("Cannot access column without name");
            if (name.Length == 1) return GetColumnOrdinal(name[0]);

            var resolvedName = name
                .Take(name.Length - 1)
                .SelectMany(x =>
                    scope.Aliases.TryGetValue(x, out var resolved)
                        ? resolved
                        : new[] { x })
                .Concat(new[] { name.Last() })
                .ToArray();

            for (var i = 0; i < Columns.Count; ++i)
                if (name != null && name.SequenceEqual(Columns[i].Name, Naming.Comparer))
                    return i;

            throw new Exception($"Column \"{string.Join(".", name)}\" does not exist in row");
        }

        public object GetValue(string name) =>
            Values[GetColumnOrdinal(name)];

        public void SetValue(string name, object value) =>
            Values[GetColumnOrdinal(name)] = value;
    }
}
