using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Evaluation;

namespace PocketSql
{
    // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/declare-cursor-transact-sql
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
            // TODO: raise error if already open/closed/deallocated?

            results = Eval.Evaluate(query, env).ResultSet;
        }

        public void Close()
        {
            // TODO: what do?
        }

        public void Deallocate()
        {
            // TODO: what do?
        }

        public DataRow FetchNext(Env env)
        {
            // TODO: raise errors if already closed/deallocated?

            if (results == null || index >= results.Rows.Count)
            {
                // env.FetchStatus = ERROR; // @@FETCH_STATUS
                return null;
            }

            // env.FetchStatus = SUCCESS; // @@FETCH_STATUS
            return results.Rows[index++];
        }
    }
}
