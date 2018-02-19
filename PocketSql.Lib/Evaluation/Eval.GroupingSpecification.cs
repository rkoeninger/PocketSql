using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(GroupingSpecification groupingSpec, RowArgument row, Scope scope)
        {
            switch (groupingSpec)
            {
                case ExpressionGroupingSpecification expr:
                    return Evaluate(expr.Expression, row, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(groupingSpec);
            }
        }
    }
}
