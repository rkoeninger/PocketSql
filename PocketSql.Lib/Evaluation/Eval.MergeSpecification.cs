using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(MergeSpecification merge, Scope scope)
        {
            var (targetTable, scope2) = Evaluate(merge.Target, null, scope);
            scope =
                merge.TableAlias == null
                    ? scope2
                    : scope2.PushAlias(
                        merge.TableAlias.Value,
                        scope.ExpandTableName(
                            ((NamedTableReference)merge.Target).SchemaObject.Identifiers
                                .Select(x => x.Value)
                                .ToArray()));
            var (sourceTable, scope3) = Evaluate(merge.TableReference, null, scope);
            scope = scope3;
            var rowCount = 0;
            var matched = new List<Row>();
            var notMatchedByTarget = new List<Row>();
            var notMatchedBySource = new List<Row>();
            var matchedClauses = merge.ActionClauses
                .Where(x => x.Condition == MergeCondition.Matched)
                .ToList();
            var notMatchedByTargetClauses = merge.ActionClauses
                .Where(x => x.Condition == MergeCondition.NotMatchedByTarget
                    || x.Condition == MergeCondition.NotMatched)
                .ToList();
            var notMatchedBySourceClauses = merge.ActionClauses
                .Where(x => x.Condition == MergeCondition.NotMatchedBySource)
                .ToList();

            if (matchedClauses.Any() || notMatchedByTargetClauses.Any())
            {
                foreach (var s in sourceTable.Rows)
                {
                    var rows = targetTable.Rows
                        .Select(t => InnerRow(sourceTable, s, targetTable, t, scope))
                        .Where(row =>
                            Evaluate(
                                merge.SearchCondition,
                                new RowArgument(row),
                                scope))
                        .ToList();

                    if (matchedClauses.Any() && rows.Any())
                    {
                        matched.AddRange(rows);
                    }
                    else if (notMatchedByTargetClauses.Any())
                    {
                        notMatchedByTarget.Add(LeftRow(sourceTable, s, targetTable, scope));
                    }
                }
            }

            if (notMatchedBySourceClauses.Any())
            {
                foreach (var t in targetTable.Rows)
                {
                    var isMatched = sourceTable.Rows.Any(s =>
                        Evaluate(
                            merge.SearchCondition,
                            new RowArgument(InnerRow(sourceTable, s, targetTable, t, scope)),
                            scope));

                    if (!isMatched)
                    {
                        notMatchedBySource.Add(RightRow(sourceTable, targetTable, t, scope));
                    }
                }
            }

            // TODO: what order should merge actions be applied?

            // TODO: build proper output sink
            var sink = new NullOutputSink();

            foreach (var clause in matchedClauses)
            {
                foreach (var row in matched)
                {
                    if (clause.SearchCondition == null
                        || Evaluate(clause.SearchCondition, new RowArgument(row), scope))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, sink, scope);
                    }
                }
            }

            foreach (var clause in notMatchedByTargetClauses)
            {
                foreach (var row in notMatchedByTarget)
                {
                    if (clause.SearchCondition == null
                        || Evaluate(clause.SearchCondition, new RowArgument(row), scope))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, sink, scope);
                    }
                }
            }

            foreach (var clause in notMatchedBySourceClauses)
            {
                foreach (var row in notMatchedBySource)
                {
                    if (Evaluate(clause.SearchCondition, new RowArgument(row), scope))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, sink, scope);
                    }
                }
            }

            // TODO: output

            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
