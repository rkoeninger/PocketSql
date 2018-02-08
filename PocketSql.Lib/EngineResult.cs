using System.Data;

namespace PocketSql
{
    public class EngineResult
    {
        public EngineResult() { }

        public EngineResult(int recordsAffected)
        {
            RecordsAffected = recordsAffected;
        }

        public EngineResult(DataTable resultSet)
        {
            ResultSet = resultSet;
        }

        public int RecordsAffected { get; set; } = -1;
        public DataTable ResultSet { get; set; }
        public object Scalar => ResultSet.Rows[0].ItemArray[0];
    }
}
