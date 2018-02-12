using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(WhileStatement loop, Env env)
        {
            EngineResult result = null;

            while (Evaluate(loop.Predicate, NullArgument.It, env))
            {
                result = Evaluate(loop.Statement, env);
            }

            return result;
        }
    }
}
