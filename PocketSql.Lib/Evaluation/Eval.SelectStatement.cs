using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SelectStatement select, Scope scope)
        {
            var results = Evaluate(select.QueryExpression, scope);

            if (select.Into != null)
            {
                scope.Env.Tables.Declare(select.Into.Identifiers.Last().Value, results.ResultSet);
                scope.Env.RowCount = results.ResultSet.Rows.Count;
                return new EngineResult(results.ResultSet.Rows.Count);
            }

            return results;
        }
    }
}
