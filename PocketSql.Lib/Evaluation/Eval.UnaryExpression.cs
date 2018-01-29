using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(UnaryExpressionType op, object value)
        {
            switch (op)
            {
                case UnaryExpressionType.Positive:
                    return value;
                case UnaryExpressionType.Negative:
                    return -1 * (decimal)value;
                case UnaryExpressionType.BitwiseNot:
                    return ~(int)value;
            }

            throw new NotImplementedException();
        }
    }
}
