using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketSql
{
    public interface IDataRow
    {
        SqlValue this[string[] columnName] { get; set; }
    }
}
