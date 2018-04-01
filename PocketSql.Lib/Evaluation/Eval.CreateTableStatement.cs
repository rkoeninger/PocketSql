using System.Linq;
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
            var table = new Table { Name = tableName };

            foreach (var c in createTable.Definition.ColumnDefinitions)
            {
                table.Columns.Add(new Column
                {
                    Name = new[]
                    {
                        databaseName,
                        schemaName,
                        tableName,
                        c.ColumnIdentifier.Value
                    }.Where(x => x != null).ToArray(),
                    Type = TranslateDbType(c.DataType)
                });

                if (c.DefaultConstraint != null)
                {
                    var env = scope.Env;
                    table.Defaults[c.DefaultConstraint.Column?.Value ?? c.ColumnIdentifier.Value] =
                        () => Evaluate(c.DefaultConstraint.Expression, NullArgument.It, new Scope(env));
                }

                if (c.IdentityOptions != null)
                {
                    table.IdentityColumnName = c.ColumnIdentifier.Value;
                    table.IdentityValue =
                        c.IdentityOptions.IdentitySeed != null
                            ? Evaluate<int>(c.IdentityOptions.IdentitySeed, NullArgument.It, scope)
                            : 1;
                    table.IdentityIncrement =
                        c.IdentityOptions.IdentityIncrement != null
                            ? Evaluate<int>(c.IdentityOptions.IdentityIncrement, NullArgument.It, scope)
                            : 1;
                }
            }

            var database = scope.Env.Engine.Databases.GetOrAdd(databaseName, Database.Named);
            var schema = database.Schemas.GetOrAdd(schemaName, Schema.Named);
            schema.Tables.Declare(table);
            return null;
        }
    }
}
