using System.Data;

namespace PocketSql.Modeling
{
    public class Column : INamed
    {
        public string Name { get; set; }
        public string[] Qualifiers { get; set; }
        public DbType Type { get; set; }
        public bool Nullable { get; set; }
    }
}
