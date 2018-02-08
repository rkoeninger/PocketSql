using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql
{
    public class View : INamed
    {
        public string Name { get; set; }
        public QueryExpression Query { get; set; }
    }
}
