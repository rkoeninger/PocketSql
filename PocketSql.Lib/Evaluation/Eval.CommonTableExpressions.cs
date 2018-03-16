using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static Scope Evaluate(IEnumerable<CommonTableExpression> exprs, Scope scope) =>
            exprs.Aggregate(
                scope,
                (s, expr) => s.PushCte(
                    expr.ExpressionName.Value,
                    Evaluate(expr.QueryExpression, s).ResultSet));
    }
}
