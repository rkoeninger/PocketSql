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
                    if (left is int && right is int)
                        return (int) left + (int) right;
                    return (int)left + (int)right;
                case BinaryExpressionType.Subtract:
                    return (int)left - (int)right;
                case BinaryExpressionType.Multiply:
                    return (int)left * (int)right;
                case BinaryExpressionType.Divide:
                    return (int)left / (int)right;
                case BinaryExpressionType.Modulo:
                    return (int)left % (int)right;
                case BinaryExpressionType.BitwiseAnd:
                    return (int)left & (int)right;
                case BinaryExpressionType.BitwiseOr:
                    return (int)left & (int)right;
                case BinaryExpressionType.BitwiseXor:
                    return (int)left ^ (int)right;
                default:
                    throw FeatureNotSupportedException.Value(op);
            }
        }
    }
}
