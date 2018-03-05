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
                var resultRow = projection.NewRow();

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
                    CopyOnto(row, tableCopy);
                }
            }

            // GROUP BY

            if (querySpec.GroupByClause != null)
            {
                // TODO: rollup, cube, grouping sets

                var groups = tableCopy.Rows.GroupBy(row =>
                    EquatableList.Of(querySpec.GroupByClause.GroupingSpecifications
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
                CopyOnto(projection, temp);
                projection.Rows.Clear();

                foreach (var item in temp.Rows
                    .Select(r => EquatableList.Of(temp.Columns
                        .Select(c => (c.Name.LastOrDefault(), r.GetValue(c.Name.LastOrDefault())))))
                    .Distinct())
                {
                    var row = projection.NewRow();

                    foreach (var i in Enumerable.Range(0, item.Elements.Count))
                    {
                        row.Values[i] = item.Elements[i].Item2;
                    }
                }
            }

            // ORDER BY

            if (querySpec.OrderByClause != null)
            {
                var elements = querySpec.OrderByClause.OrderByElements;
                var firstElement = elements.First();
                var restElements = elements.Skip(1);
                var temp = projection.CopyLayout();

                foreach (var row in restElements.Aggregate(
                    Order(projection.Rows, firstElement, scope),
                    (orderedRows, element) => Order(orderedRows, element, scope)))
                {
                    CopyOnto(row, temp);
                }

                projection.Rows.Clear();
                CopyOnto(temp, projection);
            }

            // OFFSET

            if (querySpec.OffsetClause != null)
            {
                var offset = (int)Evaluate(querySpec.OffsetClause.OffsetExpression, NullArgument.It, scope);
                var fetch = (int)Evaluate(querySpec.OffsetClause.FetchExpression, NullArgument.It, scope);

                for (var i = 0; i < offset; ++i)
                {
                    projection.Rows.RemoveAt(0);
                }

                while (projection.Rows.Count > fetch)
                {
                    projection.Rows.RemoveAt(projection.Rows.Count - 1);
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
                var resultRow = target.NewRow();

                for (var i = 0; i < selections.Count; ++i)
                {
                    resultRow.Values[i] = evaluate(selections[i].Item3, row, scope);
                }
            }
        }

        private static void CopyOnto(Table source, Table target)
        {
            foreach (var row in source.Rows)
            {
                CopyOnto(row, target);
            }
        }

        private static void CopyOnto(Row row, Table target)
        {
            var copy = target.NewRow();

            foreach (var i in Enumerable.Range(0, target.Columns.Count))
            {
                copy.Values[i] = row.Values[i];
            }
        }

        private static IOrderedEnumerable<Row> Order(
            IEnumerable<Row> seq,
            ExpressionWithSortOrder element,
            Scope scope)
        {
            object Func(Row x) => Evaluate(element.Expression, new RowArgument(x), scope);
            return element.SortOrder == SortOrder.Descending ? seq.OrderByDescending(Func) : seq.OrderBy(Func);
        }

        private static IOrderedEnumerable<Row> Order(
            IOrderedEnumerable<Row> seq,
            ExpressionWithSortOrder element,
            Scope scope)
        {
            object Func(Row x) => Evaluate(element.Expression, new RowArgument(x), scope);
            return element.SortOrder == SortOrder.Descending ? seq.ThenByDescending(Func) : seq.ThenBy(Func);
        }
    }
}
