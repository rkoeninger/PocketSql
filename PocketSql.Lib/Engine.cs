using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql
{
    public class Engine
    {
        private readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        public DataSet Evalute(StatementList statements)
        {
            return new DataSet();
        }
    }
}
