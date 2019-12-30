using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DeleteSpecification delete, IOutputSink sink, Scope scope)
        {
            var tableRef = (NamedTableReference)delete.Target;
            var table = scope.Env.Tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;

            foreach (var row in table.Rows.ToList())
            {
                if (delete.WhereClause == null
                    || Evaluate(delete.WhereClause.SearchCondition, new RowArgument(row), scope))
                {
                    table.Rows.Remove(row);
                    rowCount++;
                }

                sink.Deleted(row, scope.Env);
            }

            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
