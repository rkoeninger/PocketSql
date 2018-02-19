using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static T Evaluate<T>(ScalarExpression expr, IArgument arg, Env env) =>
            (T)Evaluate(expr, arg, env);

        public static object Evaluate(ScalarExpression expr, IArgument arg, Env env)
        {
            switch (expr)
            {
                case ParenthesisExpression paren:
                    return Evaluate(paren.Expression, arg, env);
                case IntegerLiteral intLiteral:
                    return int.Parse(intLiteral.Value);
                case NumericLiteral numericExpr:
                    return decimal.Parse(numericExpr.Value);
                case StringLiteral stringExpr:
                    return stringExpr.Value;
                case UnaryExpression unaryExpr:
                    return Evaluate(
                        unaryExpr.UnaryExpressionType,
                        Evaluate(unaryExpr.Expression, arg, env));
                case BinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, arg, env),
                        Evaluate(binaryExpr.SecondExpression, arg, env));
                case ColumnReferenceExpression colExpr:
                    switch (arg)
                    {
                        case RowArgument row:
                            return row.Value.GetValue(colExpr.MultiPartIdentifier.Identifiers.Last().Value);
                        case GroupArgument group:
                            return group.Key.Elements.First(x => x.Item1.Similar(colExpr.MultiPartIdentifier.Identifiers.Last().Value)).Item2;
                        default:
                            throw FeatureNotSupportedException.Subtype(arg);
                    }
                case VariableReference varRef:
                    return env.Vars[varRef.Name];
                case GlobalVariableExpression globRef:
                    return env.GetGlobal(globRef.Name);
                case CaseExpression caseExpr:
                    return Evaluate(caseExpr, arg, env);
                case FunctionCall funCall:
                    return Evaluate(funCall, arg, env);
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }
        }
    }
}
