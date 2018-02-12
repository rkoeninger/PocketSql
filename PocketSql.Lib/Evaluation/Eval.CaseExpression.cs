using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(CaseExpression expr, IArgument arg, Env env) =>
            Evaluate(MatchingClause(expr, arg, env)?.ThenExpression ?? expr.ElseExpression, arg, env);

        private static WhenClause MatchingClause(CaseExpression expr, IArgument arg, Env env)
        {
            switch (expr)
            {
                case SimpleCaseExpression simple:
                    var input = Evaluate(simple.InputExpression, arg, env);
                    return simple.WhenClauses
                        .FirstOrDefault(x => Equality.Equal(input, Evaluate(x.WhenExpression, arg, env)));
                case SearchedCaseExpression searched:
                    return searched.WhenClauses
                        .FirstOrDefault(x => Evaluate(x.WhenExpression, arg, env));
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }
        }
    }
}
