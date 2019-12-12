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

            var result = dml switch
            {
                InsertSpecification insert => Evaluate(insert, sink, scope),
                MergeSpecification merge => Evaluate(merge, sink, scope),
                DeleteSpecification delete => Evaluate(delete, sink, scope),
                UpdateSpecification update => Evaluate(update, sink, scope),
                _ => throw FeatureNotSupportedException.Subtype(dml)
            };

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
