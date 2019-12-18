using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(IList<SetClause> clauses, Row targetRow, Row sourceRow, IOutputSink sink, Scope scope)
        {
            var originalTargetRow = targetRow.Copy();

            foreach (var clause in clauses)
            {
                switch (clause)
                {
                    case AssignmentSetClause set:
                        var columnName = set.Column.MultiPartIdentifier.Identifiers.Last().Value;
                        targetRow.Values[targetRow.GetColumnOrdinal(columnName)] = Evaluate(
                            set.AssignmentKind,
                            sourceRow.Values[sourceRow.GetColumnOrdinal(columnName)],
                            Evaluate(set.NewValue, new RowArgument(sourceRow), scope));
                        break;
                    default:
                        throw FeatureNotSupportedException.Subtype(clause);
                }
            }

            sink.Updated(originalTargetRow, targetRow, scope.Env);
        }
    }
}
