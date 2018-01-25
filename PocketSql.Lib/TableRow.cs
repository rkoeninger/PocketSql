using System.Collections.Generic;
using System.Linq;

namespace PocketSql
{
    public class TableRow : IDataRow
    {
        private class TableCell
        {
            public string Name;
            public SqlValue Value;
        }

        private readonly IList<TableCell> cells = new List<TableCell>();

        private TableCell GetCell(string name) =>
            cells.FirstOrDefault(x => x.Name.Similar(name));

        public SqlValue this[string[] key]
        {
            get => GetCell(key.Last()).Value;
            set => GetCell(key.Last()).Value = value;
        }
    }
}
