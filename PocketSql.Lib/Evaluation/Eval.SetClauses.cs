using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(IList<SetClause> clauses, DataRow row, DataTable output, Env env)
        {
            foreach (var clause in clauses)
            {
                var oldValues = output == null ? null : new Dictionary<string, object>();

                switch (clause)
                {
                    case AssignmentSetClause set:
                        var columnName = set.Column.MultiPartIdentifier.Identifiers.Last().Value;
                        if (output != null) oldValues[columnName] = row[columnName];
                        row[columnName] = Evaluate(
                            set.AssignmentKind,
                            row[columnName],
                            Evaluate(set.NewValue, new RowArgument(row), env));
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
