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
                    return (decimal)left > (decimal)right;
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return (decimal)left >= (decimal)right;
                case BooleanComparisonType.LessThan:
                    return (decimal)left < (decimal)right;
                case BooleanComparisonType.LessThanOrEqualTo:
                    return (decimal)left <= (decimal)right;
            }

            throw new NotImplementedException();
        }
    }
}
