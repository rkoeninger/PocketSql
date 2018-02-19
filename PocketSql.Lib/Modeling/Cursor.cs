using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Evaluation;

namespace PocketSql.Modeling
{
    public class Cursor
    {
        public Cursor(QueryExpression query, bool scroll)
        {
            this.query = query;
            this.scroll = scroll;
        }

        private readonly QueryExpression query;
        private readonly bool scroll;
        private Table results;
        private int index;
        private bool open;

        public void Open(Scope scope)
        {
            if (open) throw new InvalidOperationException("Cursor already open");
            results = Eval.Evaluate(query, scope).ResultSet;
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

        public Row MoveFirst() => Access(_ => 0, false);
        public Row MoveLast() => Access(_ => results.Rows.Count - 1);
        public Row MoveNext() => Access(x => x + 1, false);
        public Row MovePrior() => Access(x => x - 1);
        public Row MoveAbsolute(int offset) => Access(_ => offset);
        public Row MoveRelative(int offset) => Access(x => x + offset);
        
        private Row Access(Func<int, int> f, bool requiresScroll = true)
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
