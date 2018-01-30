using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(
            DataTable table,
            IList<ColumnReferenceExpression> cols,
            DataTable selectedRows,
            Env env)
        {
            foreach (DataRow valuesExpr in selectedRows.Rows)
            {
                var row = table.NewRow();

                foreach (var col in cols)
                {
                    var columnName = col.MultiPartIdentifier.Identifiers[0].Value;
                    row[columnName] = valuesExpr[columnName];
                }

                table.Rows.Add(row);
            }

            return new EngineResult
            {
                RecordsAffected = selectedRows.Rows.Count
            };
        }
    }
}
