using System;
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

        private static readonly Type[] NumericTypes =
        {
            typeof(int), typeof(short), typeof(long), typeof(byte), typeof(float), typeof(double), typeof(decimal)
        };

        // TODO: refactor how built-in functions are implemented
        private static object UngroupedFunctionCall(FunctionCall funCall, IArgument arg, Scope scope)
        {
            var name = funCall.FunctionName.Value;
            var paramCount = funCall.Parameters.Count;

            // TODO: how to handle `select count(*)` ?
            switch (name.ToLower())
            {
                case "error_number" when paramCount == 0:
                    return scope.Env.ErrorNumber;
                case "error_state" when paramCount == 0:
                    return scope.Env.ErrorState;
                case "error_message" when paramCount == 0:
                    return scope.Env.ErrorMessage;
                case "error_severity" when paramCount == 0:
                    return null; // not properly implemented
                case "error_line" when paramCount == 0:
                    return null; // not properly implemented
                case "error_procedure" when paramCount == 0:
                    return null; // not properly implemented
                case "isnumeric" when paramCount == 1:
                    var value = Evaluate(funCall.Parameters[0], arg, scope);
                    return value != null && NumericTypes.Contains(value.GetType()) ? 1 : 0;
                // TODO: not supported in version 8? see unit test
                case "isnull" when paramCount == 2:
                    return Evaluate<object>(funCall.Parameters[0], arg, scope)
                        ?? Evaluate<object>(funCall.Parameters[1], arg, scope);
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
                case "dateadd":
                    Func<DateTime, int, DateTime> dateAdd = null;
                    switch (funCall.Parameters[0])
                    {
                        case ColumnReferenceExpression colExpr:
                            var dateUnit = colExpr.MultiPartIdentifier.Identifiers.LastOrDefault()?.Value?.ToLower();
                            // TODO: break this out into another function
                            switch (dateUnit)
                            {
                                case "year":
                                case "yy":
                                case "yyyy":
                                    dateAdd = (d, x) => d.AddYears(x);
                                    break;
                                case "quarter":
                                case "qq":
                                case "q":
                                    dateAdd = (d, x) => d.AddMonths(x * 3);
                                    break;
                                case "month":
                                case "mm":
                                case "m":
                                    dateAdd = (d, x) => d.AddMonths(x);
                                    break;
                                case "week":
                                case "wk":
                                case "ww":
                                    dateAdd = (d, x) => d.AddDays(x * 7);
                                    break;
                                case "dayofyear":
                                case "dy":
                                case "y":
                                case "weekday":
                                case "dw":
                                case "w":
                                case "day":
                                case "dd":
                                case "d":
                                    dateAdd = (d, x) => d.AddDays(x);
                                    break;
                                case "hour":
                                case "hh":
                                    dateAdd = (d, x) => d.AddHours(x);
                                    break;
                                case "minute":
                                case "mi":
                                case "n":
                                    dateAdd = (d, x) => d.AddMinutes(x);
                                    break;
                                case "second":
                                case "ss":
                                case "s":
                                    dateAdd = (d, x) => d.AddSeconds(x);
                                    break;
                                case "millisecond":
                                case "ms":
                                    dateAdd = (d, x) => d.AddMilliseconds(x);
                                    break;
                                default:
                                    throw FeatureNotSupportedException.Value(dateUnit, "datepart");
                            }
                            break;
                    }

                    if (dateAdd == null) throw new Exception("invalid DATEADD() time increment argument");
                    var x1 = Evaluate<int>(funCall.Parameters[1], arg, scope);
                    var d1 = Evaluate<DateTime>(funCall.Parameters[2], arg, scope);
                    return dateAdd(d1, x1);
                case "newid":
                    return Guid.NewGuid();
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
