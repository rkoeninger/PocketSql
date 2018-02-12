using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(MergeSpecification merge, Env env)
        {
            var targetTableRef = (NamedTableReference)merge.Target;
            var targetTable = env.Tables[targetTableRef.SchemaObject.BaseIdentifier.Value];
            var sourceTableRef = (NamedTableReference)merge.TableReference;
            var sourceTable = env.Tables[sourceTableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;
            var matched = new List<DataRow>();
            var notMatchedByTarget = new List<DataRow>();
            var notMatchedBySource = new List<DataRow>();

            // TODO: only search for and build up collections that will be required by match clauses

            // find matched and not matched (by target)
            foreach (DataRow row in sourceTable.Rows)
            {
                // TODO: need to respect table aliases and combine rows
                // TODO: need to define new row classes that aggregate rows via aliases
                var targetRow = targetTable.Rows.Cast<DataRow>().FirstOrDefault(x =>
                    Evaluate(merge.SearchCondition, new RowArgument(row), env));

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
            foreach (DataRow row in targetTable.Rows)
            {
                var sourceRow = sourceTable.Rows.Cast<DataRow>().FirstOrDefault(x =>
                    Evaluate(merge.SearchCondition, new RowArgument(row), env));

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
                    if (Evaluate(clause.SearchCondition, new RowArgument(row), env))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, env);
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
                    if (Evaluate(clause.SearchCondition, new RowArgument(row), env))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, env);
                    }
                }
            }

            // apply not matched by source
            foreach (var clause in merge.ActionClauses.Where(x =>
                x.Condition == MergeCondition.NotMatchedBySource))
            {
                foreach (var row in notMatchedBySource)
                {
                    if (Evaluate(clause.SearchCondition, new RowArgument(row), env))
                    {
                        rowCount++;
                        Evaluate(clause.Action, targetTable, row, env);
                    }
                }
            }

            // TODO: output into

            env.RowCount = rowCount;
            return new EngineResult(rowCount);
        }
    }
}
