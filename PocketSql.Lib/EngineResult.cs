using System.Data;

namespace PocketSql
{
    public class EngineResult
    {
        public int RecordsAffected { get; set; } = -1;
        public DataTable ResultSet { get; set; }
    }
}
