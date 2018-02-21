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

        public Row NewRow()
        {
            var row = new Row
            {
                Columns = Columns.ToList(),
                Values = Enumerable.Repeat((object)DBNull.Value, Columns.Count).ToList()
            };
            Rows.Add(row);
            return row;
        }

        public Column GetColumn(int ordinal) => Columns[ordinal];

        public Column GetColumn(string name) =>
            Columns.FirstOrDefault(c => name != null && name.Similar(c.Name.LastOrDefault()))
                ?? throw new Exception($"Column \"{name}\" does not exist in table \"{Name}\"");

        public int GetColumnOrdinal(string name)
        {
            for (var i = 0; i < Columns.Count; ++i)
                if (name != null && name.Similar(Columns[i].Name.LastOrDefault()))
                    return i;

            throw new Exception($"Column \"{name}\" does not exist in table \"{Name}\"");
        }

        public Table CopyLayout() =>
            new Table
            {
                Name = Name,
                Columns = Columns.ToList()
            };

        // TODO: public Table Select(Func<Row, Row> selector)
        // TODO: public Table Where(Func<Row, bool> filter)
        // TODO: public Table OrderBy(clauses)
    }
}
