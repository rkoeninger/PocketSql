using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DeclareVariableStatement declare, Env env)
        {
            foreach (var declaration in declare.Declarations)
            {
                env.Vars.Declare(
                    declaration.VariableName.Value,
                    declaration.Value == null ? null : Evaluate(declaration.Value, env));
            }

            return null;
        }
    }
}
