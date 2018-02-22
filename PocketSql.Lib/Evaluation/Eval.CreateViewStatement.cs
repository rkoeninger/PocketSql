using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(CreateViewStatement createView, Scope scope)
        {
            var databaseName = createView.SchemaObjectName.DatabaseIdentifier?.Value ?? scope.Env.DefaultDatabase;
            var schemaName = createView.SchemaObjectName.SchemaIdentifier?.Value ?? scope.Env.DefaultSchema;
            var viewName = createView.SchemaObjectName.BaseIdentifier.Value;
            var view = new View
            {
                Name = viewName,
                Query = createView.SelectStatement.QueryExpression
            };
            var database = scope.Env.Engine.Databases.GetOrAdd(databaseName, Database.Named);
            var schema = database.Schemas.GetOrAdd(schemaName, Schema.Named);
            schema.Views.Declare(view);
            return null;
        }
    }
}
