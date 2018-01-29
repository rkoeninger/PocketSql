using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DeleteStatement delete, Env env)
        {
            var tableRef = (NamedTableReference)delete.DeleteSpecification.Target;
            var table = env.Engine.tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;

            foreach (DataRow row in table.Rows)
            {
                if (delete.DeleteSpecification.WhereClause == null
                    || Evaluate(delete.DeleteSpecification.WhereClause.SearchCondition, row, env))
                {
                    table.Rows.Remove(row);
                    rowCount++;
                }
            }

            return new EngineResult
            {
                RecordsAffected = rowCount
            };
        }
    }
}
