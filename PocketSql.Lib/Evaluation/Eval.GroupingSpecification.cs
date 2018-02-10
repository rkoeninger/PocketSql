using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(GroupingSpecification groupingSpec, RowArgument row, Env env)
        {
            switch (groupingSpec)
            {
                case ExpressionGroupingSpecification expr:
                    return Evaluate(expr.Expression, row, env);
                default:
                    throw FeatureNotSupportedException.Subtype(groupingSpec);
            }
        }
    }
}
