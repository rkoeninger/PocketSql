using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(CreateTableStatement createTable, Env env)
        {
            var table = new DataTable();

            foreach (var column in createTable.Definition.ColumnDefinitions)
            {
                table.Columns.Add(new DataColumn
                {
                    ColumnName = column.ColumnIdentifier.Value,
                    DataType = TranslateType(column.DataType),
                    //MaxLength = 
                    //AllowDbNull = 
                });
            }

            env.Tables.Declare(createTable.SchemaObjectName.BaseIdentifier.Value, table);
            return null;
        }
    }
}
