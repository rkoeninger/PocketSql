using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(TruncateTableStatement truncate, Env env)
        {
            // TODO: partition ranges
            var table = env.Tables[truncate.TableName.BaseIdentifier.Value];
            var rowCount = table.Rows.Count;
            table.Rows.Clear();
            env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
