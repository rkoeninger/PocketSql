using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(IList<SetClause> clauses, Row row, Table output, Scope scope)
        {
            foreach (var clause in clauses)
            {
                var oldValues = output == null ? null : new Dictionary<string, object>();

                switch (clause)
                {
                    case AssignmentSetClause set:
                        var columnName = set.Column.MultiPartIdentifier.Identifiers.Last().Value;
                        if (output != null) oldValues[columnName] = row.GetColumn(columnName);
                        row.Values[row.GetColumnOrdinal(columnName)] = Evaluate(
                            set.AssignmentKind,
                            row.Values[row.GetColumnOrdinal(columnName)],
                            Evaluate(set.NewValue, new RowArgument(row), scope));
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
