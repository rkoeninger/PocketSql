using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SetVariableStatement set, Env env)
        {
            env[set.Variable.Name] = Evaluate(set.Expression, (DataRow)null, env);
            return null;
        }
    }
}
