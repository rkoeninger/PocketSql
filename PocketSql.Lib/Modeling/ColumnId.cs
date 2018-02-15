namespace PocketSql.Modeling
{
    public class ColumnId
    {
        public ColumnId(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class AliasedColumnId : ColumnId
    {
        public AliasedColumnId(string alias, string name)
            : base(name)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }

    public class TableQualifiedColumnId : ColumnId
    {
        public TableQualifiedColumnId(string table, string name)
            : base(name)
        {
            Table = table;
        }

        public string Table { get; }
    }

    public class SchemaQualifiedColumnId : TableQualifiedColumnId
    {
        public SchemaQualifiedColumnId(string schema, string table, string name)
            : base(table, name)
        {
            Schema = schema;
        }

        public string Schema { get; }
    }

    public class DatabaseQualifiedColumnId : SchemaQualifiedColumnId
    {
        public DatabaseQualifiedColumnId(string database, string schema, string table, string name)
            : base(schema, table, name)
        {
            Database = database;
        }

        public string Database { get; }
    }
}
