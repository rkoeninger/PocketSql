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
                case GroupArgument group:
                    return GroupedFunctionCall(funCall, group, scope);
                case NullArgument _:
                case RowArgument _:
                    return UngroupedFunctionCall(funCall, arg, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(arg);
            }
        }

        private static object GroupedFunctionCall(FunctionCall funCall, GroupArgument group, Scope scope)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;
            var rows = group.Rows.Select(x => new RowArgument(x));

            switch (name.ToLower())
            {
                case "sum" when paramCount == 1:
                    var expr0 = funCall.Parameters[0];
                    return rows.Sum(x => Evaluate<int>(expr0, x, scope));
                case "avg" when paramCount == 1:
                    var expr1 = funCall.Parameters[0];
                    return rows.Average(x => Evaluate<int>(expr1, x, scope));
                case "min" when paramCount == 1:
                    var expr2 = funCall.Parameters[0];
                    return rows.Min(x => Evaluate<int>(expr2, x, scope));
                case "max" when paramCount == 1:
                    var expr3 = funCall.Parameters[0];
                    return rows.Max(x => Evaluate<int>(expr3, x, scope));
                case "count" when paramCount == 1:
                    var expr4 = funCall.Parameters[0];
                    return rows.Count(x => Evaluate(expr4, x, scope) != null);
                case "count_big" when paramCount == 1:
                    var expr5 = funCall.Parameters[0];
                    return rows.LongCount(x => Evaluate(expr5, x, scope) != null);
                default:
                    throw FeatureNotSupportedException.Value(funCall);
            }
        }

        private static object UngroupedFunctionCall(FunctionCall funCall, IArgument arg, Scope scope)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;

            switch (name.ToLower())
            {
                case "lower" when paramCount == 1:
                    return Evaluate<string>(funCall.Parameters[0], arg, scope)?.ToLower();
                case "upper" when paramCount == 1:
                    return Evaluate<string>(funCall.Parameters[0], arg, scope)?.ToUpper();
                case "trim" when paramCount == 1:
                    return Evaluate<string>(funCall.Parameters[0], arg, scope)?.Trim();
                case "ltrim" when paramCount == 1:
                    return Evaluate<string>(funCall.Parameters[0], arg, scope)?.TrimStart();
                case "rtrim" when paramCount == 1:
                    return Evaluate<string>(funCall.Parameters[0], arg, scope)?.TrimEnd();
                case "reverse" when paramCount == 1:
                    var s5 = Evaluate<string>(funCall.Parameters[0], arg, scope);
                    return s5 == null ? null : new string(s5.Reverse().ToArray());
                case "substring" when paramCount == 3:
                    var s6 = Evaluate<string>(funCall.Parameters[0], arg, scope);
                    var i6 = Evaluate<int>(funCall.Parameters[1], arg, scope);
                    var j6 = Evaluate<int>(funCall.Parameters[2], arg, scope);
                    return s6?.Substring(i6, j6);
                default:
                    var env2 = scope.Env.Fork();
                    var f = scope.Env.Functions[name];

                    foreach (var (param, argExpr) in
                        f.Parameters.Zip(funCall.Parameters, (param, argExpr) => (param, argExpr)))
                    {
                        env2.Vars.Declare(param.Key, Evaluate(argExpr, arg, scope));
                    }

                    Evaluate(f.Statements, new Scope(env2));
                    return env2.ReturnValue;
            }
        }
    }
}
