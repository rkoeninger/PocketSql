﻿using Microsoft.SqlServer.TransactSql.ScriptDom;

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
                    return -1 * (int)value;
                case UnaryExpressionType.BitwiseNot:
                    return ~(int)value;
                default:
                    throw FeatureNotSupportedException.Value(op);
            }
        }
    }
}