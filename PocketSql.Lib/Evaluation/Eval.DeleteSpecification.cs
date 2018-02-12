using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DeleteSpecification delete, Env env)
        {
            var tableRef = (NamedTableReference)delete.Target;
            var table = env.Tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;

            foreach (DataRow row in table.Rows)
            {
                if (delete.WhereClause == null
                    || Evaluate(delete.WhereClause.SearchCondition, new RowArgument(row), env))
                {
                    table.Rows.Remove(row);
                    rowCount++;
                }
            }

            env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
