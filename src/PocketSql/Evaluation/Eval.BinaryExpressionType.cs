using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(BinaryExpressionType op, object left, object right)
        {
            // TODO: use switch expression, expression bodied member
            switch (op)
            {
                case BinaryExpressionType.Add:
                    // TODO: handle numeric conversions properly
                    if (left is string s && right is string t)
                        return s + t;
                    if (left is int i && right is int j)
                        return i + j;
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
