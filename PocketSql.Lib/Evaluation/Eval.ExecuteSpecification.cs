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
            var env2 = env.Fork();

            // TODO: match parameters provided with declared
            foreach (var param in exec.ExecutableEntity.Parameters)
            {
                env2.Vars[param.Variable.Name] = Evaluate(param.ParameterValue, env);
            }

            // TODO: what about multiple results?
            var results = Evaluate(proc.Statements, env2).FirstOrDefault();

            foreach (var param in exec.ExecutableEntity.Parameters.Where(x => x.IsOutput))
            {
                env.Vars[Naming.Parameter(param.Variable.Name)] = env.Vars[Naming.Parameter(param.Variable.Name)];
            }

            env.ReturnValue = env2.ReturnValue;
            return results;
        }
    }
}
