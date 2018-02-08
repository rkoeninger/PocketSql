using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(
            Procedure proc,
            IList<(string, bool, bool, object)> args,
            Env env)
        {
            var env2 = env.Fork();

            // TODO: match parameters provided with declared
            foreach (var (name, input, _, value) in args)
            {
                if (input)
                {
                    env2.Vars[Naming.Parameter(name)] = value;
                }
            }

            // TODO: what about multiple results?
            var result = Evaluate(proc.Statements, env2).FirstOrDefault();

            foreach (var (name, _, output, _) in args)
            {
                if (output)
                {
                    env.Vars[Naming.Parameter(name)] = env2.Vars[Naming.Parameter(name)];
                }
            }

            env.ReturnValue = env2.ReturnValue;
            return result;
        }
    }
}
