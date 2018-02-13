using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public class Table : INamed
    {
        public string Name { get; set; }
        public IList<Column> Columns { get; } = new List<Column>();
        public IList<Row> Rows { get; } = new List<Row>();

        public Column GetColumn(int ordinal) => Columns[ordinal];

        public Column GetColumn(string name) =>
            Columns.FirstOrDefault(c => c.Name.Similar(name))
                ?? throw new Exception($"Column \"{name}\" does not exist in table \"{Name}\"");
    }
}
