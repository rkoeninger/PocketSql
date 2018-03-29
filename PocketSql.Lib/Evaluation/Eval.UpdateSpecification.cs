using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(UpdateSpecification update, IOutputSink sink, Scope scope)
        {
            var tableRef = (NamedTableReference)update.Target;
            var table = scope.Env.Tables[tableRef.SchemaObject.BaseIdentifier.Value];

            var rowCount = 0;

            foreach (var row in table.Rows)
            {
                if (update.WhereClause == null
                    || Evaluate(update.WhereClause.SearchCondition, new RowArgument(row), scope))
                {
                    Evaluate(update.SetClauses, row, row, sink, scope);
                    rowCount++;
                }
            }

            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
