using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(BinaryExpressionType op, object left, object right)
        {
            switch (op)
            {
                case BinaryExpressionType.Add:
                    if (left is string && right is string)
                        return (string)left + (string)right;
                    return (decimal)left + (decimal)right;
                case BinaryExpressionType.Subtract:
                    return (decimal)left - (decimal)right;
                case BinaryExpressionType.Multiply:
                    return (decimal)left * (decimal)right;
                case BinaryExpressionType.Divide:
                    return (decimal)left / (decimal)right;
                case BinaryExpressionType.Modulo:
                    return (decimal)left % (decimal)right;
                case BinaryExpressionType.BitwiseAnd:
                    return (int)left & (int)right;
                case BinaryExpressionType.BitwiseOr:
                    return (int)left & (int)right;
                case BinaryExpressionType.BitwiseXor:
                    return (int)left ^ (int)right;
            }

            throw new NotImplementedException();
        }
    }
}
