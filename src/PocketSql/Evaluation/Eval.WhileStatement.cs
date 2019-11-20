using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(WhileStatement loop, Scope scope)
        {
            EngineResult result = null;

            while (Evaluate(loop.Predicate, NullArgument.It, scope))
            {
                result = Evaluate(loop.Statement, scope);
            }

            return result;
        }
    }
}
