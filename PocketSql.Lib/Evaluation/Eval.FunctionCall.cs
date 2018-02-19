using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(FunctionCall funCall, IArgument arg, Scope scope)
        {
            switch (arg)
            {
                case NullArgument nil:
                    return Evaluate(funCall, nil, scope);
                case RowArgument row:
                    return Evaluate(funCall, row, scope);
                case GroupArgument group:
                    return Evaluate(funCall, group, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(arg);
            }
        }

        public static object Evaluate(FunctionCall funCall, NullArgument nil, Scope scope)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;

            switch (name.ToLower())
            {
                case "lower" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, scope)).ToLower();
                case "upper" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, scope)).ToUpper();
                case "trim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, scope)).Trim();
                case "ltrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, scope)).TrimStart();
                case "rtrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], nil, scope)).TrimEnd();
                default:
                    var env2 = scope.Env.Fork();
                    var f = scope.Env.Functions[name];

                    foreach (var (param, arg) in f.Parameters.Zip(funCall.Parameters, (param, arg) => (param, arg)))
                    {
                        env2.Vars.Declare(param.Key, Evaluate(arg, nil, scope));
                    }

                    Evaluate(f.Statements, new Scope(env2));
                    return env2.ReturnValue;
            }
        }

        public static object Evaluate(FunctionCall funCall, RowArgument row, Scope scope)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;

            switch (name.ToLower())
            {
                case "lower" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, scope)).ToLower();
                case "upper" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, scope)).ToUpper();
                case "trim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, scope)).Trim();
                case "ltrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, scope)).TrimStart();
                case "rtrim" when paramCount == 1:
                    return ((string)Evaluate(funCall.Parameters[0], row, scope)).TrimEnd();
                default:
                    var env2 = scope.Env.Fork();
                    var f = scope.Env.Functions[name];

                    foreach (var (param, arg) in f.Parameters.Zip(funCall.Parameters, (param, arg) => (param, arg)))
                    {
                        env2.Vars.Declare(param.Key, Evaluate(arg, row, scope));
                    }

                    Evaluate(f.Statements, new Scope(env2));
                    return env2.ReturnValue;
            }
        }

        public static object Evaluate(FunctionCall funCall, GroupArgument group, Scope scope)
        {
            if (funCall.FunctionName.Value.Similar("sum") && funCall.Parameters.Count == 1)
            {
                var expr = funCall.Parameters[0];
                return group.Rows.Sum(x => Evaluate<int>(expr, new RowArgument(x), scope));
            }

            throw FeatureNotSupportedException.Value(funCall);
        }
    }
}
