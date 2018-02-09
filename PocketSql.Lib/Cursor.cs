using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Evaluation;

namespace PocketSql
{
    public class Cursor
    {
        public Cursor(QueryExpression query, bool scroll)
        {
            this.query = query;
            this.scroll = scroll;
        }

        private readonly QueryExpression query;
        private bool scroll;
        private DataTable results;
        private int index;
        private bool open = false;

        public void Open(Env env)
        {
            if (open) throw new InvalidOperationException("Cursor already open");
            results = Eval.Evaluate(query, env).ResultSet;
            open = true;
        }

        public void Close()
        {
            if (!open) throw new InvalidOperationException("Cursor already closed");
            open = false;
        }

        public void Deallocate()
        {
            if (open) throw new InvalidOperationException("Cursor still open");
            results = null;
        }

        public DataRow MoveFirst() => Access(false, _ => 0);
        public DataRow MoveLast() => Access(true, _ => results.Rows.Count - 1);
        public DataRow MoveNext() => Access(false, x => x + 1);
        public DataRow MovePrior() => Access(true, x => x - 1);
        public DataRow MoveAbsolute(int offset) => Access(true, _ => offset);
        public DataRow MoveRelative(int offset) => Access(true, x => x + offset);

        private DataRow Access(bool requiresScroll, Func<int, int> f)
        {
            if (requiresScroll && !scroll) throw new InvalidOperationException("Cusor must be scroll cursor to fetch last, prior, absolute, relative");
            if (!open) throw new InvalidOperationException("Cursor has been closed");
            index = Math.Max(-1, Math.Min(results.Rows.Count, f(index)));
            return index >= 0 && index < results.Rows.Count
                ? results.Rows[index]
                : null;
        }
    }
}
