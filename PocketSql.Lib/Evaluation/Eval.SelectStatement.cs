using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SelectStatement select, Env env)
        {
            var results = Evaluate(select.QueryExpression, env);

            if (select.Into != null)
            {
                env.Tables.Declare(select.Into.Identifiers.Last().Value, results.ResultSet);
                return new EngineResult(results.ResultSet.Rows.Count);
            }

            return results;
        }
    }
}
