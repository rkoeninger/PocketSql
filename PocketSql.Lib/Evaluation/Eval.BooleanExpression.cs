using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static bool Evaluate(BooleanExpression expr, IArgument arg, Env env)
        {
            switch (expr)
            {
                case BooleanParenthesisExpression paren:
                    return Evaluate(paren.Expression, arg, env);
                case BooleanBinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, arg, env),
                        Evaluate(binaryExpr.SecondExpression, arg, env));
                case BooleanTernaryExpression ternaryExpr:
                    return Evaluate(
                        ternaryExpr.TernaryExpressionType,
                        Evaluate(ternaryExpr.FirstExpression, arg, env),
                        Evaluate(ternaryExpr.SecondExpression, arg, env),
                        Evaluate(ternaryExpr.ThirdExpression, arg, env));
                case BooleanComparisonExpression compareExpr:
                    return Evaluate(
                        compareExpr.ComparisonType,
                        Evaluate(compareExpr.FirstExpression, arg, env),
                        Evaluate(compareExpr.SecondExpression, arg, env));
                case BooleanNotExpression notExpr:
                    return !Evaluate(notExpr.Expression, arg, env);
                case BooleanIsNullExpression isNullExpr:
                    return Evaluate(isNullExpr.Expression, arg, env) == null;
                case InPredicate inExpr:
                    var value = Evaluate(inExpr.Expression, arg, env);
                    return value != null && inExpr.Values.Any(x => value.Equals(Evaluate(x, arg, env)));
                case ExistsPredicate existsExpr:
                    return Evaluate(existsExpr.Subquery.QueryExpression, env).ResultSet.Rows.Count > 0;
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }
        }
    }
}
