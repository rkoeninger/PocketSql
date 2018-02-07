using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Evaluation;

namespace PocketSql
{
    public class Cursor
    {
        public Cursor(QueryExpression query)
        {
            this.query = query;
        }

        private readonly QueryExpression query;
        private DataTable results;
        private int index;

        public void Open(Env env)
        {
            results = Eval.Evaluate(query, env).ResultSet;
        }

        public DataRow FetchNext(Env env)
        {
            if (index >= results.Rows.Count)
            {
                // env.FetchStatus = ERROR;
                return null;
            }

            // env.FetchStatus = SUCCESS;
            return results.Rows[index++];
        }
    }
}
