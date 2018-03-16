using System.Collections.Generic;

namespace PocketSql.Modeling
{
    public interface IArgument { }

    public class RowArgument : IArgument
    {
        public RowArgument(Row value)
        {
            Value = value;
        }

        public Row Value { get; set; }
    }

    public class GroupArgument : IArgument
    {
        public GroupArgument(EquatableAssociationList key, List<Row> rows)
        {
            Key = key;
            Rows = rows;
        }

        public EquatableAssociationList Key { get; set; }
        public List<Row> Rows { get; set; }
    }

    public class NullArgument : IArgument
    {
        public static NullArgument It = new NullArgument();
    }
}
