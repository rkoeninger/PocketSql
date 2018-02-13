using System.Collections.Generic;

namespace PocketSql.Modeling
{
    public class Row
    {
        public IList<object> Values { get; } = new List<object>();
    }
}
