using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SetVariableStatement set, Env env)
        {
            env[set.Variable.Name] = Evaluate(set.Expression, null, env);
            return null;
        }
    }
}
