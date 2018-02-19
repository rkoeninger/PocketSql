using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(QueryExpression queryExpr, Scope scope)
        {
            // TODO: offset/fetch should be done here?
            //       order only gets applied if not already ordered
            //       QuerySpecification could contain TopRowFilter,
            //       which gets applied after order by

            switch (queryExpr)
            {
                case QuerySpecification querySpec:
                    return Evaluate(querySpec, scope);
                case QueryParenthesisExpression paren:
                    // TODO: need to handle surrounding offset/fetch?
                    return Evaluate(paren.QueryExpression, scope);
                case BinaryQueryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryQueryExpressionType,
                        binaryExpr.All,
                        Evaluate(binaryExpr.FirstQueryExpression, scope).ResultSet,
                        Evaluate(binaryExpr.SecondQueryExpression, scope).ResultSet);
                default:
                    throw FeatureNotSupportedException.Subtype(queryExpr);
            }
        }
    }
}
