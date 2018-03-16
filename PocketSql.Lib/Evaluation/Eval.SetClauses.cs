using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(IList<SetClause> clauses, Row row, Table output, Scope scope) =>
            Evaluate(clauses, row, row, output, scope);

        public static void Evaluate(IList<SetClause> clauses, Row targetRow, Row sourceRow, Table output, Scope scope)
        {
            foreach (var clause in clauses)
            {
                var oldValues = output == null ? null : new Dictionary<string, object>();

                switch (clause)
                {
                    case AssignmentSetClause set:
                        var columnName = set.Column.MultiPartIdentifier.Identifiers.Last().Value;
                        if (output != null) oldValues[columnName] = targetRow.GetColumn(columnName);
                        targetRow.Values[targetRow.GetColumnOrdinal(columnName)] = Evaluate(
                            set.AssignmentKind,
                            sourceRow.Values[sourceRow.GetColumnOrdinal(columnName)],
                            Evaluate(set.NewValue, new RowArgument(sourceRow), scope));
                        break;
                    default:
                        throw FeatureNotSupportedException.Subtype(clause);
                }

                if (output != null)
                {
                    // TODO: add row to output, differentiating between inserted. and deleted.
                }
            }
        }
    }
}
