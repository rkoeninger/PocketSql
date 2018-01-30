using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(QuerySpecification querySpec, Env env)
        {
            // First the product of all tables in the from clause is formed.
            // The where clause is then evaluated to eliminate rows that do not satisfy the search_condition.
            // Next, the rows are grouped using the columns in the group by clause.
            // Then, Groups that do not satisfy the search_condition in the having clause are eliminated.
            // Next, the expressions in the select clause target list are evaluated.
            // If the distinct keyword in present in the select clause, duplicate rows are now eliminated.
            // The union is taken after each sub-select is evaluated.
            // Finally, the resulting rows are sorted according to the columns specified in the order by clause.

            var tableRef = (NamedTableReference)querySpec.FromClause?.TableReferences?.Single();
            var table = tableRef == null ? null : env.Engine.tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var projection = new DataTable();

            var selections = querySpec.SelectElements.SelectMany(s =>
            {
                switch (s)
                {
                    // TODO: respect table alias in star expression
                    case SelectStarExpression star:
                        return table.Columns.Cast<DataColumn>().Select(c =>
                            (c.ColumnName,
                                c.DataType,
                                (ScalarExpression)CreateColumnReferenceExpression(c.ColumnName)));
                    case SelectScalarExpression scalar:
                        return new[]
                        {
                            (scalar.ColumnName?.Value ?? InferName(scalar.Expression),
                                InferType(scalar.Expression, table),
                                scalar.Expression)
                        }.AsEnumerable();
                    default:
                        throw new NotImplementedException();
                }
            }).ToList();

            foreach (var (name, type, _) in selections)
            {
                projection.Columns.Add(new DataColumn
                {
                    ColumnName = name,
                    DataType = type
                });
            }

            foreach (var row in table?.Rows.Cast<DataRow>() ?? new DataRow[] { null })
            {
                if (querySpec.WhereClause == null || Evaluate(querySpec.WhereClause.SearchCondition, row, env))
                {
                    var resultRow = projection.NewRow();

                    for (var i = 0; i < selections.Count; ++i)
                    {
                        resultRow[i] = Evaluate(selections[i].Item3, row, env);
                    }

                    projection.Rows.Add(resultRow);
                }
            }

            // TODO: need to perform group by before evaluating select expressions

            if (querySpec.GroupByClause != null)
            {
                var rows = projection.Rows.Cast<DataRow>().ToList();
                var temp = projection.Clone();

                // TODO: rollup, cube, grouping sets

                EquatableList KeyBuilder(DataRow row)
                {
                    var keys = new EquatableList();

                    foreach (ExpressionGroupingSpecification g in
                        querySpec.GroupByClause.GroupingSpecifications)
                    {
                        keys.Elements.Add(Evaluate(g.Expression, row, env));
                    }

                    return keys;
                }

                // rows.GroupBy(KeyBuilder)

                if (querySpec.HavingClause != null)
                {
                    foreach (DataRow row in temp.Rows)
                    {
                        if (!Evaluate(querySpec.HavingClause.SearchCondition, row, env))
                        {
                            temp.Rows.Remove(row);
                        }
                    }
                }

                projection.Rows.Clear();
                CopyOnto(temp, projection);
            }

            if (querySpec.UniqueRowFilter == UniqueRowFilter.Distinct)
            {
                var temp = projection.Clone();
                CopyOnto(projection, temp);
                projection.Rows.Clear();

                foreach (var item in temp.Rows.Cast<DataRow>()
                    .Select(r => EquatableList.Of(r.ItemArray))
                    .Distinct())
                {
                    var row = projection.NewRow();

                    foreach (DataColumn col in temp.Columns)
                    {
                        row[col.Ordinal] = item.Elements[col.Ordinal];
                    }

                    projection.Rows.Add(row);
                }
            }

            if (querySpec.OrderByClause != null)
            {
                var elements = querySpec.OrderByClause.OrderByElements;
                var firstElement = elements.First();
                var restElements = elements.Skip(1);
                var rows = projection.Rows.Cast<DataRow>().ToList();
                var temp = projection.Clone();

                foreach (var row in restElements.Aggregate(
                    Order(rows, firstElement, env),
                    (orderedRows, element) => Order(orderedRows, element, env)))
                {
                    CopyOnto(row, temp);
                }

                projection.Rows.Clear();
                CopyOnto(temp, projection);
            }

            if (querySpec.OffsetClause != null)
            {
                var offset = (int)Evaluate(querySpec.OffsetClause.OffsetExpression, null, env);
                var fetch = (int)Evaluate(querySpec.OffsetClause.FetchExpression, null, env);

                for (var i = 0; i < offset; ++i)
                {
                    projection.Rows.RemoveAt(0);
                }

                while (projection.Rows.Count > fetch)
                {
                    projection.Rows.RemoveAt(projection.Rows.Count - 1);
                }
            }

            return new EngineResult
            {
                ResultSet = projection
            };
        }

        private static void CopyOnto(DataTable source, DataTable target)
        {
            foreach (DataRow row in source.Rows)
            {
                CopyOnto(row, target);
            }
        }

        private static void CopyOnto(DataRow row, DataTable target)
        {
            var copy = target.NewRow();

            foreach (DataColumn col in target.Columns)
            {
                copy[col.Ordinal] = row[col.Ordinal];
            }

            target.Rows.Add(copy);
        }

        private static IOrderedEnumerable<DataRow> Order(
            IEnumerable<DataRow> seq,
            ExpressionWithSortOrder element,
            Env env)
        {
            object Func(DataRow x) => Evaluate(element.Expression, x, env);
            return element.SortOrder == SortOrder.Descending ? seq.OrderByDescending(Func) : seq.OrderBy(Func);
        }

        private static IOrderedEnumerable<DataRow> Order(
            IOrderedEnumerable<DataRow> seq,
            ExpressionWithSortOrder element,
            Env env)
        {
            object Func(DataRow x) => Evaluate(element.Expression, x, env);
            return element.SortOrder == SortOrder.Descending ? seq.ThenByDescending(Func) : seq.ThenBy(Func);
        }
    }
}
