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

        public Column GetColumn(string name) =>
            Columns.FirstOrDefault(c => c.Name.Similar(name))
                ?? throw new Exception($"Column \"{name}\" does not exist in row");

        public int GetColumnOrdinal(string name)
        {
            for (var i = 0; i < Columns.Count; ++i)
                if (name.Similar(Columns[i].Name))
                    return i;

            throw new Exception($"Column \"{name}\" does not exist in row");
        }

        public object GetValue(string name) =>
            Values[GetColumnOrdinal(name)];

        public void SetValue(string name, object value) =>
            Values[GetColumnOrdinal(name)] = value;
    }
}
