using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static bool Evaluate(
            BooleanTernaryExpressionType type,
            object first,
            object second,
            object third)
        {
            // TODO: use IComparable

            switch (type)
            {
                case BooleanTernaryExpressionType.Between:
                    return (int)first >= (int)second && (int)first <= (int)third;
                case BooleanTernaryExpressionType.NotBetween:
                    return (int)first < (int)second || (int)first > (int)third;
                default:
                    throw FeatureNotSupportedException.Value(type);
            }
        }
    }
}
