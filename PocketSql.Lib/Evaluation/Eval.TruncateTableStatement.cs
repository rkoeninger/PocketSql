using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(TruncateTableStatement truncate, Env env)
        {
            // TODO: partition ranges
            var table = env.Engine.tables[truncate.TableName.BaseIdentifier.Value];
            var rowCount = table.Rows.Count;
            table.Rows.Clear();

            return new EngineResult
            {
                RecordsAffected = rowCount
            };
        }
    }
}
