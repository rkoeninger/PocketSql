using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public class Table : INamed
    {
        public string Name { get; set; }
        public IList<Column> Columns { get; set; } = new List<Column>();
        public IList<Row> Rows { get; set; } = new List<Row>();
        public IDictionary<string, Func<object>> Defaults { get; } = new Dictionary<string, Func<object>>();
        public string IdentityColumnName { get; set; }
        public int IdentityValue { get; set; } = 1;
        public int IdentityIncrement { get; set; } = 1;

        public Row NewRow(Env env)
        {
            var row = new Row
            {
                Columns = Columns.ToList(),
                Values = Enumerable.Repeat((object)DBNull.Value, Columns.Count).ToList()
            };
            Rows.Add(row);

            if (IdentityColumnName != null)
            {
                env.Identity = IdentityValue;
                row.SetValue(IdentityColumnName, IdentityValue);
                IdentityValue += IdentityIncrement;
            }

            foreach (var def in Defaults)
            {
                row.SetValue(def.Key, def.Value());
            }

            return row;
        }

        public Column GetColumn(int ordinal) => Columns[ordinal];

        public Column GetColumn(string name) => GetColumn(GetColumnOrdinal(name));

        public int GetColumnOrdinal(string name)
        {
            for (var i = 0; i < Columns.Count; ++i)
                if (name != null && name.Similar(Columns[i].Name.LastOrDefault()))
                    return i;

            throw new Exception($"Column \"{name}\" does not exist in table \"{Name}\"");
        }

        // TODO: this is duplicated in Row.cs
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

            if (resolvedName.Length < 3) resolvedName = new[] { scope.Env.DefaultSchema }.Concat(resolvedName).ToArray();
            if (resolvedName.Length < 4) resolvedName = new[] { scope.Env.DefaultDatabase }.Concat(resolvedName).ToArray();

            for (var i = 0; i < Columns.Count; ++i)
                if (resolvedName.SequenceEqual(Columns[i].Name, Naming.Comparer))
                    return i;

            throw new Exception($"Column \"{string.Join(".", name)}\" does not exist in row");
        }

        public Table CopyLayout() =>
            new Table
            {
                Name = Name,
                Columns = Columns.ToList()
            };

        public Row AddCopy(Row r, Env env)
        {
            var s = NewRow(env);

            // TODO: match names instead?

            foreach (var i in Enumerable.Range(0, s.Columns.Count))
            {
                s.Values[i] = r.Values[i];
            }

            return s;
        }

        // TODO: public Table Select(Func<Row, Row> selector)
        // TODO: public Table Where(Func<Row, bool> filter)
        // TODO: public Table OrderBy(clauses)
    }
}
