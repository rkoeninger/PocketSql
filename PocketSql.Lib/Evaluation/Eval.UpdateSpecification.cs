using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(UpdateSpecification update, Scope scope)
        {
            var tableRef = (NamedTableReference)update.Target;
            var table = scope.Env.Tables[tableRef.SchemaObject.BaseIdentifier.Value];
            Table output = null;

            if (update.OutputClause != null)
            {
                // TODO: extract and share logic with Evaluate(SelectStatement, ...)
                // TODO: handle inserted.* vs deleted.* and $action
                var selections = update.OutputClause.SelectColumns
                    .SelectMany(ExtractSelection(table, scope)).ToList();

                output = new Table();

                foreach (var (name, type, _) in selections)
                {
                    output.Columns.Add(new Column
                    {
                        Name = new[] { name },
                        Type = type
                    });
                }
            }

            var rowCount = 0;

            foreach (var row in table.Rows)
            {
                if (update.WhereClause == null
                    || Evaluate(update.WhereClause.SearchCondition, new RowArgument(row), scope))
                {
                    Evaluate(update.SetClauses, row, output, scope);
                    rowCount++;
                }
            }

            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
            // TODO: ResultSet = output
        }
    }
}
