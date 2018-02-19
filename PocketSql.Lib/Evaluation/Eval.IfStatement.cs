using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(IfStatement conditional, Scope scope) =>
            Evaluate(
                Evaluate(conditional.Predicate, NullArgument.It, scope)
                    ? conditional.ThenStatement
                    : conditional.ElseStatement,
                scope);
    }
}
