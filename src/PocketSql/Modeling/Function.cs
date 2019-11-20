using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Modeling
{
    public class Function
    {
        public string[] Name { get; set; }
        public IDictionary<string, DbType> Parameters { get; set; }
        public StatementList Statements { get; set; }
        public DbType ReturnType { get; set; }
    }
}
