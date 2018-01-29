using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(AssignmentKind kind, object current, object value)
        {
            switch (kind)
            {
                case AssignmentKind.Equals:
                    return value;
                case AssignmentKind.AddEquals:
                    // TODO: handle numeric conversions properly
                    if (current is int && value is int)
                        return (int)current + (int)value;
                    return (decimal)current + (decimal)value;
                case AssignmentKind.SubtractEquals:
                    return (decimal)current - (decimal)value;
                case AssignmentKind.MultiplyEquals:
                    return (decimal)current * (decimal)value;
                case AssignmentKind.DivideEquals:
                    return (decimal)current / (decimal)value;
                case AssignmentKind.ModEquals:
                    return (decimal)current % (decimal)value;
                case AssignmentKind.BitwiseAndEquals:
                    return (int)current & (int)value;
                case AssignmentKind.BitwiseOrEquals:
                    return (int)current | (int)value;
                case AssignmentKind.BitwiseXorEquals:
                    return (int)current ^ (int)value;
            }

            throw new Exception($"Not a valid assignment kind: {kind}");
        }
    }
}
