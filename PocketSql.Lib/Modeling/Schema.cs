using System.Data;

namespace PocketSql.Modeling
{
    public class Schema : INamed
    {
        public Schema(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Namespace<Function> Functions { get; } = new Namespace<Function>();
        public Namespace<Procedure> Procedures { get; } = new Namespace<Procedure>();
        public Namespace<Table> Tables { get; } = new Namespace<Table>();
        public Namespace<View> Views { get; } = new Namespace<View>();
    }
}
