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
            ValuesInsertSource values,
            Env env)
        {
            foreach (var valuesExpr in values.RowValues)
            {
                var row = table.NewRow();

                for (var i = 0; i < cols.Count; ++i)
                {
                    var name = cols[i].MultiPartIdentifier.Identifiers[0].Value;
                    row.SetValue(name, Evaluate(valuesExpr.ColumnValues[i], NullArgument.It, env));
                }
            }

            env.RowCount = values.RowValues.Count;
            return new EngineResult(values.RowValues.Count);
        }
    }
}
