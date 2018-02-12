using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(CreateViewStatement createView, Env env)
        {
            env.Views.Declare(new View
            {
                Name = createView.SchemaObjectName.BaseIdentifier.Value,
                Query = createView.SelectStatement.QueryExpression
            });
            return null;
        }
    }
}
