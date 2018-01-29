using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static bool Evaluate(BooleanBinaryExpressionType op, bool left, bool right)
        {
            // TODO: emulate sql server short-circuit behavior (may be version specific)
            switch (op)
            {
                case BooleanBinaryExpressionType.And:
                    return left && right;
                case BooleanBinaryExpressionType.Or:
                    return left || right;
            }

            throw new NotImplementedException();
        }
    }
}
