using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: return output if output clause is present
        public static void Evaluate(MergeAction action, Table targetTable, Row row, Scope scope)
        {
            Row GetTargetRow() => row.Sources[EquatableArray.Of(scope.ExpandTableName(new[] { targetTable.Name }))];

            switch (action)
            {
                case InsertMergeAction insert:
                    Evaluate(targetTable, insert.Columns, insert.Source, new RowArgument(row), scope);
                    return;
                case UpdateMergeAction update:
                    Evaluate(update.SetClauses, GetTargetRow(), row, null, scope);
                    return;
                case DeleteMergeAction _:
                    targetTable.Rows.Remove(GetTargetRow());
                    return;
                default:
                    throw FeatureNotSupportedException.Subtype(action);
            }
        }
    }
}
