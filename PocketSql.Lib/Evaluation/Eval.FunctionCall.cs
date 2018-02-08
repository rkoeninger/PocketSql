using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(FunctionCall funCall, DataRow row, Env env)
        {
            // TODO: built-in functions

            var f = env.Functions[funCall.FunctionName.Value];
            var env2 = env.Fork();

            foreach (var (param, arg) in f.Parameters.Zip(funCall.Parameters, (param, arg) => (param, arg)))
            {
                env2.Vars.Declare(param.Key, Evaluate(arg, row, env));
            }

            Evaluate(f.Statements, env2);
            return env2.ReturnValue;
        }

        public static object Evaluate(FunctionCall funCall, IGrouping<EquatableList, DataRow> row, Env env)
        {
            if (funCall.FunctionName.Value.Similar("sum") && funCall.Parameters.Count == 1)
            {
                var expr = funCall.Parameters[0];
                return row.Select(x => Evaluate(expr, x, env)).Cast<int>().Sum();
            }

            throw new NotImplementedException();
        }
    }
}
