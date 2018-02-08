using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(ExecuteSpecification exec, Env env)
        {
            // TODO: exec.ExecuteContext for permissions
            // TODO: exec.LinkedServer for DB
            var execRef = (ExecutableProcedureReference)exec.ExecutableEntity;
            var procName = execRef.ProcedureReference.ProcedureReference.Name.Identifiers.Last().Value;
            var proc = env.Procedures[procName];
            return Evaluate(
                proc,
                execRef.Parameters
                    .Select(p => (p.Variable.Name, !p.IsOutput, p.IsOutput, Evaluate(p.ParameterValue, env)))
                    .ToList(),
                env);
        }
    }
}
