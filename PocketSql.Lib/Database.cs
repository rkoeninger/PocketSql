namespace PocketSql
{
    public class Database : INamed
    {
        public Database(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Namespace<Schema> Schemas { get; } = new Namespace<Schema>();
    }
}
