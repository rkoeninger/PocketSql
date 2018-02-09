using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(CursorStatement statement, Env env)
        {
            // TODO: global cursors? statement.Cursor.IsGlobal
            var name = statement.Cursor.Name.Value;
            var cursor = (Cursor)env.Vars[name];

            switch (statement)
            {
                case OpenCursorStatement _:
                    cursor.Open(env);
                    return;
                case CloseCursorStatement _:
                    cursor.Close();
                    return;
                case DeallocateCursorStatement _:
                    cursor.Deallocate();
                    return;
                case FetchCursorStatement fetch:
                    var result = CursorFetch(fetch, cursor, env);
                    env.FetchStatus = result == null ? 1 : 0;
                    if (env.FetchStatus != 0) return;

                    foreach (var (v, x) in fetch.IntoVariables.Zip(result.ItemArray, (v, x) => (v, x)))
                    {
                        env.Vars.DeclareOrSet(v.Name, x);
                    }

                    return;
            }

            throw new System.Exception();
        }

        private static DataRow CursorFetch(FetchCursorStatement fetch, Cursor cursor, Env env)
        {
            switch (fetch.FetchType?.Orientation ?? FetchOrientation.None)
            {
                case FetchOrientation.None:
                case FetchOrientation.Next:
                    return cursor.MoveNext();
                case FetchOrientation.Prior:
                    return cursor.MovePrior();
                case FetchOrientation.First:
                    return cursor.MoveFirst();
                case FetchOrientation.Last:
                    return cursor.MoveLast();
                case FetchOrientation.Absolute:
                    return cursor.MoveAbsolute((int)Evaluate(fetch.FetchType.RowOffset, env));
                case FetchOrientation.Relative:
                    return cursor.MoveRelative((int)Evaluate(fetch.FetchType.RowOffset, env));
            }

            throw new System.Exception();
        }
    }
}
