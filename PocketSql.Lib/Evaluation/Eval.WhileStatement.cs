using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(WhileStatement loop, Env env)
        {
            while (Evaluate(loop.Predicate, null, env))
            {
                Evaluate(loop.Statement, env);
            }

            return null;
        }
    }
}
