using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: use switch expression, expression bodied member
        public static bool Evaluate(BooleanComparisonType op, object left, object right)
        {
            switch (op)
            {
                case BooleanComparisonType.Equals:
                    return left != null && left.Equals(right);
                case BooleanComparisonType.NotEqualToBrackets:
                case BooleanComparisonType.NotEqualToExclamation:
                    return left == null || !left.Equals(right);
                case BooleanComparisonType.GreaterThan:
                    // TODO: handle numeric conversions properly
                    if (left is int i && right is int j)
                        return i > j;
                    if (left is decimal m && right is decimal n)
                        return m > n;
                    return string.Compare(left.ToString(), right.ToString()) > 0;

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    if (left is int i2 && right is int j2)
                        return i2 >= j2;
                    if (left is decimal m2 && right is decimal n2)
                        return m2 >= n2;
                    return string.Compare(left.ToString(), right.ToString()) >= 0;

                case BooleanComparisonType.LessThan:
                    if (left is int i3 && right is int j3)
                        return i3 < j3;
                    if (left is decimal m3 && right is decimal n3)
                        return m3 < n3;
                    return string.Compare(left.ToString(), right.ToString()) < 0;

                case BooleanComparisonType.LessThanOrEqualTo:
                    if (left is int i4 && right is int j4)
                        return i4 <= j4;
                    if (left is decimal m4 && right is decimal n4)
                        return m4 <= n4;
                    return string.Compare(left.ToString(), right.ToString()) <= 0;
                default:
                    throw FeatureNotSupportedException.Value(op);
            }
        }
    }
}
