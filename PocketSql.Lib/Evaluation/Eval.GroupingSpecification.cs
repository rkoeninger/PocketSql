using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(GroupingSpecification groupingSpec, DataRow row, Env env)
        {
            switch (groupingSpec)
            {
                case ExpressionGroupingSpecification expr:
                    return Evaluate(expr.Expression, row, env);
            }

            throw new NotImplementedException();
        }
    }
}
