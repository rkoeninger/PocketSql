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
            var targetTableRef = (NamedTableReference)merge.Target;
            var targetTable = scope.Env.Tables[targetTableRef.SchemaObject.BaseIdentifier.Value];
            var sourceTableRef = (NamedTableReference)merge.TableReference;
            var sourceTable = scope.Env.Tables[sourceTableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;
            var matched = new List<Row>();
            var notMatchedByTarget = new List<Row>();
            var notMatchedBySource = new List<Row>();
            var matchedClauses = merge.ActionClauses
                .Where(x => x.Condition == MergeCondition.Matched)
                .ToList();
            var notMatchedByTargetClauses = merge.ActionClauses
                .Where(x => x.Condition == MergeCondition.NotMatchedByTarget
                    || x.Condition == MergeCondition.Matched)
                .ToList();
            var notMatchedBySourceClauses = merge.ActionClauses
                .Where(x => x.Condition == MergeCondition.NotMatchedBySource)
                .ToList();

            if (matchedClauses.Any() || notMatchedByTargetClauses.Any())
            {
                foreach (var s in sourceTable.Rows)
                {
                    // TODO: need to respect table aliases and combine rows
                    // TODO: need to define new row classes that aggregate rows via aliases
                    var rows = targetTable.Rows
                        .Select(t => InnerRow(s, t))
                        .Where(row =>
                            Evaluate(merge.SearchCondition, new RowArgument(row), scope))
                        .ToList();

                    if (matchedClauses.Any() && rows.Any())
                    {
                        matched.AddRange(rows);
                    }
                    else if (notMatchedByTargetClauses.Any())
                    {
                        notMatchedByTarget.Add(LeftRow(s, targetTable));
                    }
                }
            }

            if (notMatchedBySourceClauses.Any())
            {
                foreach (var t in targetTable.Rows)
                {
                    var isMatched = sourceTable.Rows.Any(s =>
                        Evaluate(merge.SearchCondition, new RowArgument(InnerRow(s, t)), scope));

                    if (!isMatched)
                    {
                        notMatchedBySource.Add(RightRow(sourceTable, t));
                    }
                }
            }

            // TODO: what order should merge actions be applied?

            foreach (var clause in matchedClauses)
            {
                foreach (var row in matched)
                {
                    if (clause.SearchCondition == null
                        || Evaluate(clause.SearchCondition, new RowArgument(row), scope))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, scope);
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
                        Evaluate(clause.Action, targetTable, row, scope);
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
                        Evaluate(clause.Action, targetTable, row, scope);
                    }
                }
            }

            // TODO: output

            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
