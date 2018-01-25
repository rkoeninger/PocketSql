using System.Collections.Generic;
using System.Linq;

namespace PocketSql
{
    public class JoinedRow : IDataRow
    {
        private class RowRef
        {
            public string[] Key;
            public IDataRow Row;
        }

        private readonly IList<RowRef> rows = new List<RowRef>();

        public IDataRow GetRow(string[] key)
        {
            var row = rows
                .Where(r =>
                    r.Key[0].Similar(key[0])
                    && r.Key[1].Similar(key[1])
                    && r.Key[2].Similar(key[2]))
                .ToList();
            return row.Any() ? null : row.First().Row;
        }

        public SqlValue this[string[] key]
        {
            get => GetRow(key)[key];
            set => GetRow(key)[key] = value;
        }
    }
}
