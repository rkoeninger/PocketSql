using System.Collections.Generic;
using System.Linq;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(
            Procedure proc,
            IList<(string, bool, bool, object)> args,
            Scope scope)
        {
            var env2 = scope.Env.Fork();

            // TODO: match parameters provided with declared
            foreach (var (name, input, _, value) in args)
            {
                if (input)
                {
                    env2.Vars[Naming.Parameter(name)] = value;
                }
            }

            // TODO: what about multiple results?
            var result = Evaluate(proc.Statements, new Scope(env2)).FirstOrDefault();

            foreach (var (name, _, output, _) in args)
            {
                if (output)
                {
                    scope.Env.Vars[Naming.Parameter(name)] = env2.Vars[Naming.Parameter(name)];
                }
            }

            scope.Env.ReturnValue = env2.ReturnValue;
            return result;
        }
    }
}
