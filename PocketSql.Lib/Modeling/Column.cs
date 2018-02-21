using System.Data;

namespace PocketSql.Modeling
{
    public class Column
    {
        public string[] Name { get; set; }
        public DbType Type { get; set; }
        public bool Nullable { get; set; }
    }
}
