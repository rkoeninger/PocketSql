using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(UnaryExpressionType op, object value) =>
            op switch
            {
                UnaryExpressionType.Positive => value,
                UnaryExpressionType.Negative => (-1 * (int) value),
                UnaryExpressionType.BitwiseNot => ~(int) value,
                _ => throw FeatureNotSupportedException.Value(op)
            };
    }
}
