using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(SetVariableStatement set, Scope scope)
        {
            scope.Env.Vars[set.Variable.Name] = Evaluate(
                set.AssignmentKind,
                scope.Env.Vars[set.Variable.Name],
                Evaluate(set.Expression, NullArgument.It, scope));
            return null;
        }
    }
}
