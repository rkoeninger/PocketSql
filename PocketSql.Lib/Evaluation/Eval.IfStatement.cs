using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(IfStatement conditional, Env env) =>
            Evaluate(
                Evaluate(conditional.Predicate, NullArgument.It, env)
                    ? conditional.ThenStatement
                    : conditional.ElseStatement,
                env);
    }
}
