using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DataModificationSpecification dml, Scope scope)
        {
            IOutputSink sink;

            // TODO: support scalar expressions in TableOutputSink, not just column names
            //       how to handle INSERTED. and DELETED. aliases?

            if (dml.OutputClause != null || dml.OutputIntoClause != null)
            {
                sink = new TableOutputSink(
                    (dml.OutputClause?.SelectColumns ?? dml.OutputIntoClause?.SelectColumns)?
                    .Select(s => new Column
                    {
                        Name = ((ColumnReferenceExpression)((SelectScalarExpression)s).Expression)
                            .MultiPartIdentifier.Identifiers.Select(x => x.Value).ToArray(),
                        Type = DbType.AnsiString
                    }).ToList());
            }
            else
            {
                sink = new NullOutputSink();
            }

            EngineResult result;

            switch (dml)
            {
                case InsertSpecification insert:
                    result = Evaluate(insert, sink, scope);
                    break;
                case MergeSpecification merge:
                    result = Evaluate(merge, sink, scope);
                    break;
                case DeleteSpecification delete:
                    result = Evaluate(delete, sink, scope);
                    break;
                case UpdateSpecification update:
                    result = Evaluate(update, sink, scope);
                    break;
                default:
                    throw FeatureNotSupportedException.Subtype(dml);
            }

            if (dml.OutputIntoClause != null)
            {
                var (table, scope2) = Evaluate(dml.OutputIntoClause.IntoTable, null, scope);
                Evaluate(
                    table,
                    dml.OutputIntoClause.IntoTableColumns,
                    ((TableOutputSink)sink).Output,
                    new NullOutputSink(),
                    scope2);
            }

            return dml.OutputClause != null
                ? new EngineResult(((TableOutputSink)sink).Output)
                : result;
        }
    }
}
