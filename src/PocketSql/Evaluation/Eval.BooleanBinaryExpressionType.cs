using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: emulate sql server short-circuit behavior (may be version specific)
        public static bool Evaluate(BooleanBinaryExpressionType op, bool left, bool right) =>
            op switch
            {
                BooleanBinaryExpressionType.And => (left && right),
                BooleanBinaryExpressionType.Or => (left || right),
                _ => throw FeatureNotSupportedException.Value(op)
            };
    }
}
