using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SetVariableStatement set, Env env)
        {
            env.Vars[set.Variable.Name] = Evaluate(set.Expression, env);
            return null;
        }
    }
}
