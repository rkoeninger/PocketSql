using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(IIfCall iif, IArgument arg, Scope scope) =>
            Evaluate(iif.Predicate, arg, scope)
                ? Evaluate(iif.ThenExpression, arg, scope)
                : Evaluate(iif.ElseExpression, arg, scope);
    }
}
