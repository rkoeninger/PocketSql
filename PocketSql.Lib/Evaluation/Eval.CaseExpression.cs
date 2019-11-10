using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(CaseExpression expr, IArgument arg, Scope scope) =>
            Evaluate(MatchingClause(expr, arg, scope)?.ThenExpression ?? expr.ElseExpression, arg, scope);

        private static WhenClause MatchingClause(CaseExpression expr, IArgument arg, Scope scope)
        {
            switch (expr)
            {
                case SimpleCaseExpression simple:
                    var input = Evaluate(simple.InputExpression, arg, scope);
                    return simple.WhenClauses
                        .FirstOrDefault(x => Equality.Equal(input, Evaluate(x.WhenExpression, arg, scope)));
                case SearchedCaseExpression searched:
                    return searched.WhenClauses
                        .FirstOrDefault(x => Evaluate(x.WhenExpression, arg, scope));
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }
        }
    }
}
