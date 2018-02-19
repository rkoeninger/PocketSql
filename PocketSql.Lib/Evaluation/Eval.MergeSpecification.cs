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

            // TODO: only search for and build up collections that will be required by match clauses

            // find matched and not matched (by target)
            foreach (var row in sourceTable.Rows)
            {
                // TODO: need to respect table aliases and combine rows
                // TODO: need to define new row classes that aggregate rows via aliases
                var targetRow = targetTable.Rows.FirstOrDefault(x =>
                    Evaluate(merge.SearchCondition, new RowArgument(row), scope));

                if (targetRow == null)
                {
                    notMatchedByTarget.Add(row);
                }
                else
                {
                    matched.Add(row);
                }
            }

            // find not matched by source
            foreach (var row in targetTable.Rows)
            {
                var sourceRow = sourceTable.Rows.FirstOrDefault(x =>
                    Evaluate(merge.SearchCondition, new RowArgument(row), scope));

                if (sourceRow == null)
                {
                    notMatchedBySource.Add(row);
                }
            }

            // apply matched
            foreach (var clause in merge.ActionClauses.Where(x =>
                x.Condition == MergeCondition.Matched))
            {
                foreach (var row in matched)
                {
                    if (Evaluate(clause.SearchCondition, new RowArgument(row), scope))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, scope);
                    }
                }
            }

            // TODO: what order should merge actions be applied?

            // apply not matched (by target)
            foreach (var clause in merge.ActionClauses.Where(x =>
                x.Condition == MergeCondition.NotMatched || x.Condition == MergeCondition.NotMatchedByTarget))
            {
                foreach (var row in notMatchedByTarget)
                {
                    if (Evaluate(clause.SearchCondition, new RowArgument(row), scope))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, scope);
                    }
                }
            }

            // apply not matched by source
            foreach (var clause in merge.ActionClauses.Where(x =>
                x.Condition == MergeCondition.NotMatchedBySource))
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

            // TODO: output into

            scope.Env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
