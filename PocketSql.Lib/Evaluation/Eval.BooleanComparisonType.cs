using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
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
                    if (left is int && right is int)
                        return (int)left > (int)right;
                    return (decimal)left > (decimal)right;
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    if (left is int && right is int)
                        return (int)left >= (int)right;
                    return (decimal)left >= (decimal)right;
                case BooleanComparisonType.LessThan:
                    if (left is int && right is int)
                        return (int)left < (int)right;
                    return (decimal)left < (decimal)right;
                case BooleanComparisonType.LessThanOrEqualTo:
                    if (left is int && right is int)
                        return (int)left <= (int)right;
                    return (decimal)left <= (decimal)right;
            }

            throw new NotImplementedException();
        }
    }
}
