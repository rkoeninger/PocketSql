namespace PocketSql.Modeling
{
    public class Database : INamed
    {
        public Database(string name)
        {
            Name = name;
            Schemas.Declare(new Schema("dbo"));
        }

        public string Name { get; }
        public Namespace<Schema> Schemas { get; } = new Namespace<Schema>();
    }
}
