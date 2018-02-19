using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(ExecuteSpecification exec, Scope scope)
        {
            // TODO: exec.ExecuteContext for permissions
            // TODO: exec.LinkedServer for DB
            var execRef = (ExecutableProcedureReference)exec.ExecutableEntity;
            var procName = execRef.ProcedureReference.ProcedureReference.Name.Identifiers.Last().Value;
            var proc = scope.Env.Procedures[procName];
            return Evaluate(
                proc,
                execRef.Parameters
                    .Select(p => (
                        p.Variable.Name,
                        !p.IsOutput,
                        p.IsOutput,
                        Evaluate(p.ParameterValue, NullArgument.It, scope)))
                    .ToList(),
                scope);
        }
    }
}
