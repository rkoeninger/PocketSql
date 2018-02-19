using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(CursorStatement statement, Scope scope)
        {
            // TODO: global cursors? statement.Cursor.IsGlobal
            var name = statement.Cursor.Name.Value;
            var cursor = (Cursor)scope.Env.Vars[name];

            switch (statement)
            {
                case OpenCursorStatement _:
                    cursor.Open(scope);
                    return;
                case CloseCursorStatement _:
                    cursor.Close();
                    return;
                case DeallocateCursorStatement _:
                    cursor.Deallocate();
                    return;
                case FetchCursorStatement fetch:
                    var result = CursorFetch(fetch, cursor, scope);
                    scope.Env.FetchStatus = result == null ? 1 : 0;
                    if (result == null) return;

                    foreach (var (v, x) in fetch.IntoVariables.Zip(result.Values, (v, x) => (v, x)))
                    {
                        scope.Env.Vars.DeclareOrSet(v.Name, x);
                    }

                    return;
                default:
                    throw FeatureNotSupportedException.Subtype(statement);
            }
        }

        private static Row CursorFetch(FetchCursorStatement fetch, Cursor cursor, Scope scope)
        {
            var orientation = fetch.FetchType?.Orientation ?? FetchOrientation.None;

            switch (orientation)
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
                    return cursor.MoveAbsolute(Evaluate<int>(fetch.FetchType?.RowOffset, NullArgument.It, scope));
                case FetchOrientation.Relative:
                    return cursor.MoveRelative(Evaluate<int>(fetch.FetchType?.RowOffset, NullArgument.It, scope));
                default:
                    throw FeatureNotSupportedException.Value(orientation);
            }
        }
    }
}
