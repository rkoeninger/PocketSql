using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: use IComparable
        // TODO: handle numeric conversions properly
        public static bool Evaluate(
            BooleanTernaryExpressionType type,
            object first,
            object second,
            object third) =>
            type switch
            {
                BooleanTernaryExpressionType.Between => ((int) first >= (int) second && (int) first <= (int) third),
                BooleanTernaryExpressionType.NotBetween => ((int) first < (int) second || (int) first > (int) third),
                _ => throw FeatureNotSupportedException.Value(type)
            };
    }
}
