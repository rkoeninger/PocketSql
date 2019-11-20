using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public class Row
    {
        public IDictionary<EquatableArray<string>, Row> Sources { get; set; } = new Dictionary<EquatableArray<string>, Row>();
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

        // TODO: expand name with tables in scope
        public int GetColumnOrdinal(string[] name, Scope scope)
        {
            name = name.Where(x => x != null).ToArray();

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

            // TODO: need more general purpose name resolution
            if (resolvedName.Length < 3) resolvedName = new[] { scope.Env.DefaultSchema }.Concat(resolvedName).ToArray();
            if (resolvedName.Length < 4) resolvedName = new[] { scope.Env.DefaultDatabase }.Concat(resolvedName).ToArray();

            for (var i = 0; i < Columns.Count; ++i)
                if (resolvedName.SequenceEqual(Columns[i].Name, Naming.Comparer))
                    return i;

            throw new Exception($"Column \"{string.Join(".", name)}\" does not exist in row");
        }

        public object GetValue(string name) =>
            Values[GetColumnOrdinal(name)];

        public object GetValue(string[] name, Scope scope) =>
            Values[GetColumnOrdinal(name, scope)];

        public void SetValue(string name, object value) =>
            Values[GetColumnOrdinal(name)] = value;

        public Row Copy() => new Row
        {
            Sources = Sources,
            Columns = Columns,
            Values = Values.ToList()
        };
    }
}
