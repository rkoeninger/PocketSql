using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static bool Evaluate(BooleanExpression expr, IArgument arg, Scope scope)
        {
            switch (expr)
            {
                case BooleanParenthesisExpression paren:
                    return Evaluate(paren.Expression, arg, scope);
                case BooleanBinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, arg, scope),
                        Evaluate(binaryExpr.SecondExpression, arg, scope));
                case BooleanTernaryExpression ternaryExpr:
                    return Evaluate(
                        ternaryExpr.TernaryExpressionType,
                        Evaluate(ternaryExpr.FirstExpression, arg, scope),
                        Evaluate(ternaryExpr.SecondExpression, arg, scope),
                        Evaluate(ternaryExpr.ThirdExpression, arg, scope));
                case BooleanComparisonExpression compareExpr:
                    return Evaluate(
                        compareExpr.ComparisonType,
                        Evaluate(compareExpr.FirstExpression, arg, scope),
                        Evaluate(compareExpr.SecondExpression, arg, scope));
                case BooleanNotExpression notExpr:
                    return !Evaluate(notExpr.Expression, arg, scope);
                case BooleanIsNullExpression isNullExpr:
                    return Evaluate(isNullExpr.Expression, arg, scope) == null;
                case InPredicate inExpr:
                    var value = Evaluate(inExpr.Expression, arg, scope);
                    return value != null && inExpr.Values.Any(x => value.Equals(Evaluate(x, arg, scope)));
                case ExistsPredicate existsExpr:
                    return Evaluate(existsExpr.Subquery.QueryExpression, scope).ResultSet.Rows.Count > 0;
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }
        }
    }
}
