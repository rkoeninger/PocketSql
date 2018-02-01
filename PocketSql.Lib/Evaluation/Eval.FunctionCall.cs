using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(FunctionCall funCall, DataRow row, Env env)
        {
            throw new NotImplementedException();
        }

        public static object Evaluate(FunctionCall funCall, IGrouping<EquatableList, DataRow> row, Env env)
        {
            if (funCall.FunctionName.Value.Similar("sum") && funCall.Parameters.Count == 1)
            {
                var expr = funCall.Parameters[0];
                return row.Select(x => Evaluate(expr, x, env)).Cast<int>().Sum();
            }

            throw new NotImplementedException();
        }
    }
}
