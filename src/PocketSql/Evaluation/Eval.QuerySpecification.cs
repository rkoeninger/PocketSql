using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(QuerySpecification querySpec, Scope scope)
        {
            // First the product of all tables in the from clause is formed.
            // The where clause is then evaluated to eliminate rows that do not satisfy the search_condition.
            // Next, the rows are grouped using the columns in the group by clause.
            // Then, Groups that do not satisfy the search_condition in the having clause are eliminated.
            // Next, the expressions in the select clause target list are evaluated.
            // If the distinct keyword in present in the select clause, duplicate rows are now eliminated.
            // The union is taken after each sub-select is evaluated.
            // Finally, the resulting rows are sorted according to the columns specified in the order by clause.
            // And then the offset/fetch is evaluated

            // FROM

            Table table = null;

            if (querySpec.FromClause?.TableReferences != null)
            {
                foreach (var tableRef in querySpec.FromClause.TableReferences)
                {
                    (table, scope) = Evaluate(tableRef, table, scope);
                }
            }

            var selections = querySpec.SelectElements
                .SelectMany(ExtractSelection(table, scope)).ToList();
            var projection = new Table();

            foreach (var (name, type, _) in selections)
            {
                projection.Columns.Add(new Column
                {
                    Name = new[] { name },
                    Type = type
                });
            }

            // SELECT without FROM

            if (table == null)
            {
                var resultRow = projection.NewRow(scope.Env);

                for (var i = 0; i < selections.Count; ++i)
                {
                    resultRow.Values[i] = Evaluate(selections[i].Item3, NullArgument.It, scope);
                }

                return new EngineResult(projection);
            }

            var tableCopy = table.CopyLayout();

            // WHERE

            foreach (var row in table.Rows)
            {
                if (querySpec.WhereClause == null
                    || Evaluate(querySpec.WhereClause.SearchCondition, new RowArgument(row), scope))
                {
                    CopyOnto(row, tableCopy, scope.Env);
                }
            }

            // GROUP BY

            if (querySpec.GroupByClause != null)
            {
                // TODO: rollup, cube, grouping sets

                var groups = tableCopy.Rows.GroupBy(row =>
                    EquatableAssociationList.Of(querySpec.GroupByClause.GroupingSpecifications
                        .Select(g => (InferName(g), Evaluate(g, new RowArgument(row), scope))))).ToList();

                // HAVING

                if (querySpec.HavingClause != null)
                {
                    groups = groups
                        .Where(x => Evaluate(querySpec.HavingClause.SearchCondition, new GroupArgument(x.Key, x.ToList()), scope))
                        .ToList();
                }

                // SELECT

                Select(
                    Evaluate,
                    groups.Select(x => new GroupArgument(x.Key, x.ToList())),
                    projection,
                    selections,
                    scope);
            }
            else
            {
                // SELECT

                Select(
                    Evaluate,
                    tableCopy.Rows.Select(x => new RowArgument(x)),
                    projection,
                    selections,
                    scope);
            }

            // DISTINCT

            if (querySpec.UniqueRowFilter == UniqueRowFilter.Distinct)
            {
                var temp = projection.CopyLayout();
                CopyOnto(projection, temp, scope.Env);
                projection.Rows.Clear();

                foreach (var item in temp.Rows
                    .Select(r => EquatableAssociationList.Of(temp.Columns
                        .Select(c => (c.Name.LastOrDefault(), r.GetValue(c.Name.LastOrDefault())))))
                    .Distinct())
                {
                    var row = projection.NewRow(scope.Env);

                    foreach (var i in Enumerable.Range(0, item.Elements.Count))
                    {
                        row.Values[i] = item.Elements[i].Item2;
                    }
                }
            }

            return new EngineResult(projection);
        }

        private static void Select<T>(
            Func<ScalarExpression, T, Scope, object> evaluate,
            IEnumerable<T> source,
            Table target,
            IList<(string, DbType, ScalarExpression)> selections,
            Scope scope)
        {
            foreach (var row in source)
            {
                var resultRow = target.NewRow(scope.Env);

                for (var i = 0; i < selections.Count; ++i)
                {
                    resultRow.Values[i] = evaluate(selections[i].Item3, row, scope);
                }
            }
        }
    }
}
