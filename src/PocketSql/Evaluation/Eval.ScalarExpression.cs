using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static T Evaluate<T>(ScalarExpression expr, IArgument arg, Scope scope) =>
            Evaluate(expr, arg, scope).As<T>();

        public static object Evaluate(ScalarExpression expr, IArgument arg, Scope scope)
        {
            switch (expr)
            {
                case ParenthesisExpression paren:
                    return Evaluate(paren.Expression, arg, scope);
                case IntegerLiteral intLiteral:
                    return int.Parse(intLiteral.Value);
                case NumericLiteral numericExpr:
                    return decimal.Parse(numericExpr.Value);
                case StringLiteral stringExpr:
                    return stringExpr.Value;
                case UnaryExpression unaryExpr:
                    return Evaluate(
                        unaryExpr.UnaryExpressionType,
                        Evaluate(unaryExpr.Expression, arg, scope));
                case BinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, arg, scope),
                        Evaluate(binaryExpr.SecondExpression, arg, scope));
                case ColumnReferenceExpression colExpr:
                    switch (arg)
                    {
                        case RowArgument row:
                            return row.Value.GetValue(
                                colExpr.MultiPartIdentifier.Identifiers.Select(x => x.Value).ToArray(),
                                scope);
                        case GroupArgument group:
                            return group.Key.Elements.First(x => x.Item1.Similar(colExpr.MultiPartIdentifier.Identifiers.Last().Value)).Item2;
                        default:
                            throw FeatureNotSupportedException.Subtype(arg);
                    }
                case VariableReference varRef:
                    return scope.Env.Vars[varRef.Name];
                case GlobalVariableExpression globRef:
                    return scope.Env.GetGlobal(globRef.Name);
                case CaseExpression caseExpr:
                    return Evaluate(caseExpr, arg, scope);
                case IIfCall iif:
                    return Evaluate(iif, arg, scope);
                case FunctionCall funCall:
                    return Evaluate(funCall, arg, scope);
                case NullLiteral _:
                    return null;
                case NullIfExpression nullIf:
                    return Evaluate(nullIf, arg, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }
        }
    }
}
