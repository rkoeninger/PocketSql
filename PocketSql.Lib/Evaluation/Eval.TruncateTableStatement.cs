using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(TruncateTableStatement truncate, Scope scope)
        {
            // TODO: partition ranges
            var table = scope.Env.Tables[truncate.TableName.BaseIdentifier.Value];
            var rowCount = table.Rows.Count;
            table.Rows.Clear();
            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
