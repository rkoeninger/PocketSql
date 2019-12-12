using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(GroupingSpecification groupingSpec, RowArgument row, Scope scope) =>
            groupingSpec switch
            {
                ExpressionGroupingSpecification expr => Evaluate(expr.Expression, row, scope),
                _ => throw FeatureNotSupportedException.Subtype(groupingSpec)
            };
    }
}
