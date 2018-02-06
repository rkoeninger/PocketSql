using System.Data;

namespace PocketSql
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
        public Namespace<DataTable> Tables { get; } = new Namespace<DataTable>();
    }
}
