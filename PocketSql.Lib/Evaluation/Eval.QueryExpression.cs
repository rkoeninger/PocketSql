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
            }

            throw new NotImplementedException();
        }
    }
}
