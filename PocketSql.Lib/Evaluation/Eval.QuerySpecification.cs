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
            // And then the offset/fetch is evaluated

            // FROM

            var table = querySpec.FromClause?.TableReferences?
                .Aggregate((DataTable)null, (ts, tr) => Evaluate(tr, ts, env));
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

            var projection = new DataTable();

            foreach (var (name, type, _) in selections)
            {
                projection.Columns.Add(new DataColumn
                {
                    ColumnName = name,
                    DataType = type
                });
            }

            // SELECT without FROM

            if (table == null)
            {
                var projection1 = new DataTable();

                foreach (var (name, type, _) in selections)
                {
                    projection1.Columns.Add(new DataColumn
                    {
                        ColumnName = name,
                        DataType = type
                    });
                }

                var resultRow = projection1.NewRow();

                for (var i = 0; i < selections.Count; ++i)
                {
                    resultRow[i] = Evaluate(selections[i].Item3, (DataRow)null, env);
                }

                projection1.Rows.Add(resultRow);
                return new EngineResult
                {
                    ResultSet = projection1
                };
            }

            var tableCopy = table.Clone();

            // WHERE

            foreach (DataRow row in table.Rows)
            {
                if (querySpec.WhereClause == null || Evaluate(querySpec.WhereClause.SearchCondition, row, env))
                {
                    CopyOnto(row, tableCopy);
                }
            }

            // GROUP BY

            if (querySpec.GroupByClause != null)
            {
                var rows = tableCopy.Rows.Cast<DataRow>().ToList();
                var temp = tableCopy.Clone();

                // TODO: rollup, cube, grouping sets

                var groups = rows.GroupBy(row =>
                    EquatableList.Of(querySpec.GroupByClause.GroupingSpecifications.Select(g =>
                        Evaluate(g, row, env)))).ToList();

                // HAVING

                if (querySpec.HavingClause != null)
                {
                    groups = groups.Where(x => Evaluate(querySpec.HavingClause.SearchCondition, x, env)).ToList();
                    //foreach (DataRow row in temp.Rows)
                    //{
                    //    if (!Evaluate(querySpec.HavingClause.SearchCondition, row, env))
                    //    {
                    //        temp.Rows.Remove(row);
                    //    }
                    //}
                }

                //tableCopy.Rows.Clear();
                //CopyOnto(temp, tableCopy);

                // SELECT

                Select(Evaluate, groups, projection, selections, env);
            }
            else
            {
                // SELECT

                Select(Evaluate, tableCopy.Rows.Cast<DataRow>(), projection, selections, env);
            }

            // DISTINCT

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

            // ORDER BY

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

            // OFFSET

            if (querySpec.OffsetClause != null)
            {
                var offset = (int)Evaluate(querySpec.OffsetClause.OffsetExpression, (DataRow)null, env);
                var fetch = (int)Evaluate(querySpec.OffsetClause.FetchExpression, (DataRow)null, env);

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

        private static void Select<T>(
            Func<ScalarExpression, T, Env, object> evaluate,
            IEnumerable<T> source,
            DataTable target,
            IList<(string, Type, ScalarExpression)> selections,
            Env env)
        {
            foreach (var row in source)
            {
                var resultRow = target.NewRow();

                for (var i = 0; i < selections.Count; ++i)
                {
                    resultRow[i] = evaluate(selections[i].Item3, row, env);
                }

                target.Rows.Add(resultRow);
            }
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
