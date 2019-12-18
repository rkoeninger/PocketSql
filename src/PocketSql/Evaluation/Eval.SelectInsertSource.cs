using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(
            Table table,
            IList<ColumnReferenceExpression> cols,
            Table selectedRows,
            IOutputSink sink,
            Scope scope)
        {
            foreach (var valuesExpr in selectedRows.Rows)
            {
                var row = table.NewRow(scope.Env);

                foreach (var col in cols)
                {
                    var columnName = col.MultiPartIdentifier.Identifiers[0].Value;
                    row.SetValue(columnName, valuesExpr.GetValue(columnName));
                }

                sink.Inserted(row, scope.Env);
            }

            scope.Env.RowCount = selectedRows.Rows.Count;
            return new EngineResult(selectedRows.Rows.Count);
        }
    }
}
