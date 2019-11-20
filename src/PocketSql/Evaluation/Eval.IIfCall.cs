using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(IIfCall iif, IArgument arg, Scope scope) =>
            Evaluate(
                Evaluate(iif.Predicate, arg, scope)
                    ? iif.ThenExpression
                    : iif.ElseExpression,
                arg,
                scope);
    }
}
