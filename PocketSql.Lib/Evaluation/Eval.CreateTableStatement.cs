using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(CreateTableStatement createTable, Scope scope)
        {
            var databaseName = createTable.SchemaObjectName.DatabaseIdentifier?.Value ?? scope.Env.DefaultDatabase;
            var schemaName = createTable.SchemaObjectName.SchemaIdentifier?.Value ?? scope.Env.DefaultSchema;
            var tableName = createTable.SchemaObjectName.BaseIdentifier.Value;
            var table = new Table
            {
                Name = tableName
            };

            foreach (var column in createTable.Definition.ColumnDefinitions)
            {
                table.Columns.Add(new Column
                {
                    Name = new[] { column.ColumnIdentifier.Value },
                    Type = TranslateDbType(column.DataType)
                });
            }

            var database = scope.Env.Engine.Databases.GetOrAdd(databaseName, Database.Named);
            var schema = database.Schemas.GetOrAdd(schemaName, Schema.Named);
            schema.Tables.Declare(table);
            return null;
        }
    }
}
