using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(UpdateSpecification update, Env env)
        {
            var tableRef = (NamedTableReference)update.Target;
            var table = env.Tables[tableRef.SchemaObject.BaseIdentifier.Value];
            DataTable output = null;

            if (update.OutputClause != null)
            {
                // TODO: extract and share logic with Evaluate(SelectStatement, ...)
                // TODO: handle inserted.* vs deleted.* and $action
                var selections = update.OutputClause.SelectColumns
                    .SelectMany(ExtractSelection(table, env)).ToList();

                output = new DataTable();

                foreach (var (name, type, _) in selections)
                {
                    output.Columns.Add(new DataColumn
                    {
                        ColumnName = name,
                        DataType = type
                    });
                }
            }

            var rowCount = 0;

            foreach (DataRow row in table.Rows)
            {
                if (update.WhereClause == null || Evaluate(update.WhereClause.SearchCondition, new RowArgument(row), env))
                {
                    Evaluate(update.SetClauses, row, output, env);
                    rowCount++;
                }
            }

            env.RowCount = rowCount;
            return new EngineResult(rowCount);
            // TODO: ResultSet = output
        }
    }
}
