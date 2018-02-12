using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(FunctionCall funCall, IArgument arg, Env env)
        {
            switch (arg)
            {
                case NullArgument nil:
                    return Evaluate(funCall, nil, env);
                case RowArgument row:
                    return Evaluate(funCall, row, env);
                case GroupArgument group:
                    return Evaluate(funCall, group, env);
                default:
                    throw FeatureNotSupportedException.Subtype(arg);
            }
        }

        public static object Evaluate(FunctionCall funCall, NullArgument nil, Env env)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;

            switch (name.ToLower())
            {
                case "lower" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, env)).ToLower();
                case "upper" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, env)).ToUpper();
                case "trim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, env)).Trim();
                case "ltrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, env)).TrimStart();
                case "rtrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, env)).TrimEnd();
                default:
                    var env2 = env.Fork();
                    var f = env.Functions[name];

                    foreach (var (param, arg) in f.Parameters.Zip(funCall.Parameters, (param, arg) => (param, arg)))
                    {
                        env2.Vars.Declare(param.Key, Evaluate(arg, nil, env));
                    }

                    Evaluate(f.Statements, env2);
                    return env2.ReturnValue;
            }
        }

        public static object Evaluate(FunctionCall funCall, RowArgument row, Env env)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;

            switch (name.ToLower())
            {
                case "lower" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, env)).ToLower();
                case "upper" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, env)).ToUpper();
                case "trim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, env)).Trim();
                case "ltrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, env)).TrimStart();
                case "rtrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, env)).TrimEnd();
                default:
                    var env2 = env.Fork();
                    var f = env.Functions[name];

                    foreach (var (param, arg) in f.Parameters.Zip(funCall.Parameters, (param, arg) => (param, arg)))
                    {
                        env2.Vars.Declare(param.Key, Evaluate(arg, row, env));
                    }

                    Evaluate(f.Statements, env2);
                    return env2.ReturnValue;
            }
        }

        public static object Evaluate(FunctionCall funCall, GroupArgument group, Env env)
        {
            if (funCall.FunctionName.Value.Similar("sum") && funCall.Parameters.Count == 1)
            {
                var expr = funCall.Parameters[0];
                return group.Rows.Select(x => Evaluate(expr, new RowArgument(x), env)).Cast<int>().Sum();
            }

            throw FeatureNotSupportedException.Value(funCall);
        }
    }
}
