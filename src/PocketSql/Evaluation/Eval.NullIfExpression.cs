using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: not supported in version 8? see unit test
        public static object Evaluate(NullIfExpression nullIf, IArgument arg, Scope scope)
        {
            var n1 = Evaluate<object>(nullIf.FirstExpression, arg, scope);
            var n2 = Evaluate<object>(nullIf.SecondExpression, arg, scope);
            return Equality.Equal(n1, n2) ? null : n1;
        }
    }
}
