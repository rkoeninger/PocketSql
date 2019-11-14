using PocketSql.Modeling;

namespace PocketSql
{
    public class EngineResult
    {
        public EngineResult()
        {
        }

        public EngineResult(int recordsAffected)
        {
            RecordsAffected = recordsAffected;
        }

        public EngineResult(Table resultSet)
        {
            ResultSet = resultSet;
        }

        public int RecordsAffected { get; set; } = -1;
        public Table ResultSet { get; set; }
        public object Scalar => ResultSet.Rows[0].Values[0];
    }
}
