using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SetVariableStatement set, Env env)
        {
            env.Vars[set.Variable.Name] = Evaluate(set.Expression, NullArgument.It, env);
            return null;
        }
    }
}
