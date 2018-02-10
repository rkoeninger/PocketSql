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
            ValuesInsertSource values,
            Env env)
        {
            foreach (var valuesExpr in values.RowValues)
            {
                var row = table.NewRow();

                for (var i = 0; i < cols.Count; ++i)
                {
                    row[cols[i].MultiPartIdentifier.Identifiers[0].Value] =
                        Evaluate(valuesExpr.ColumnValues[i], NullArgument.It, env);
                }

                table.Rows.Add(row);
            }

            env.RowCount = values.RowValues.Count;
            return new EngineResult(values.RowValues.Count);
        }
    }
}
