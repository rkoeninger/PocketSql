using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(CreateTableStatement createTable, Env env)
        {
            var table = new Table();

            foreach (var column in createTable.Definition.ColumnDefinitions)
            {
                table.Columns.Add(new Column
                {
                    Name = column.ColumnIdentifier.Value,
                    Type = TranslateDbType(column.DataType)
                });
            }

            env.Tables.Declare(createTable.SchemaObjectName.BaseIdentifier.Value, table);
            return null;
        }
    }
}
