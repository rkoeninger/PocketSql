using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(IfStatement conditional, Scope scope) =>
            Evaluate(conditional.Predicate, NullArgument.It, scope) ? Evaluate(conditional.ThenStatement, scope) :
            conditional.ElseStatement != null ? Evaluate(conditional.ElseStatement, scope) :
            null;
    }
}
