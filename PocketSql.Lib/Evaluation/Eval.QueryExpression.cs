using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(QueryExpression queryExpr, Env env)
        {
            switch (queryExpr)
            {
                case QuerySpecification querySpec:
                    return Evaluate(querySpec, env);
                case QueryParenthesisExpression paren:
                    // TODO: need to handle surrounding offset/fetch?
                    return Evaluate(paren.QueryExpression, env);
                case BinaryQueryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryQueryExpressionType,
                        binaryExpr.All,
                        Evaluate(binaryExpr.FirstQueryExpression, env).ResultSet,
                        Evaluate(binaryExpr.SecondQueryExpression, env).ResultSet);
            }

            throw new NotImplementedException();
        }
    }
}
