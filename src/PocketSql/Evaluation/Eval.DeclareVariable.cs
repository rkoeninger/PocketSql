using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DeclareVariableStatement declare, Scope scope)
        {
            foreach (var declaration in declare.Declarations)
            {
                scope.Env.Vars.Declare(
                    declaration.VariableName.Value,
                    declaration.Value == null ? null : Evaluate(declaration.Value, NullArgument.It, scope));
            }

            return null;
        }
    }
}
