using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(QueryExpression queryExpr, Scope scope)
        {
            EngineResult result;

            switch (queryExpr)
            {
                case QuerySpecification querySpec:
                    result = Evaluate(querySpec, scope);
                    break;
                case QueryParenthesisExpression paren:
                    result = Evaluate(paren.QueryExpression, scope);
                    break;
                case BinaryQueryExpression binaryExpr:
                    result = Evaluate(
                        binaryExpr.BinaryQueryExpressionType,
                        binaryExpr.All,
                        Evaluate(binaryExpr.FirstQueryExpression, scope).ResultSet,
                        Evaluate(binaryExpr.SecondQueryExpression, scope).ResultSet);
                    break;
                default:
                    throw FeatureNotSupportedException.Subtype(queryExpr);
            }

            if (result.ResultSet == null)
            {
                return result;
            }

            var projection = result.ResultSet;

            // ORDER BY

            if (queryExpr.OrderByClause != null)
            {
                var elements = queryExpr.OrderByClause.OrderByElements;
                var firstElement = elements.First();
                var restElements = elements.Skip(1);
                var temp = projection.CopyLayout();

                foreach (var row in restElements.Aggregate(
                    Order(projection.Rows, firstElement, scope),
                    (orderedRows, element) => Order(orderedRows, element, scope)))
                {
                    CopyOnto(row, temp);
                }

                projection.Rows.Clear();
                CopyOnto(temp, projection);
            }

            // OFFSET

            if (queryExpr.OffsetClause != null)
            {
                var offset = Evaluate<int>(queryExpr.OffsetClause.OffsetExpression, NullArgument.It, scope);

                for (var i = 0; i < offset; ++i)
                {
                    projection.Rows.RemoveAt(0);
                }

                if (queryExpr.OffsetClause.FetchExpression != null)
                {
                    var fetch = Evaluate<int>(queryExpr.OffsetClause.FetchExpression, NullArgument.It, scope);

                    while (projection.Rows.Count > fetch)
                    {
                        projection.Rows.RemoveAt(projection.Rows.Count - 1);
                    }
                }
            }

            return new EngineResult(projection);
        }
    }
}
