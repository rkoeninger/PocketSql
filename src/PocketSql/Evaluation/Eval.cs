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
        private static ColumnReferenceExpression CreateColumnReferenceExpression(string name)
        {
            var schemaObjectName = new SchemaObjectName();
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.BaseIdentifier.Value = name;

            // TODO: qualify rest of name

            return new ColumnReferenceExpression
            {
                MultiPartIdentifier = schemaObjectName
            };
        }

        private static void CopyOnto(Table source, Table target, Env env)
        {
            foreach (var row in source.Rows)
            {
                CopyOnto(row, target, env);
            }
        }

        private static void CopyOnto(Row row, Table target, Env env)
        {
            var copy = target.NewRow(env);

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

        private static Func<SelectElement, IEnumerable<(string, DbType, ScalarExpression)>>
            ExtractSelection(Table table, Scope scope) => s =>
        {
            switch (s)
            {
                // TODO: respect table alias in star expression
                // TODO: function calls like count(*) are SelectStarExpressions
                case SelectStarExpression _:
                    return table.Columns.Select(c => (
                        c.Name.LastOrDefault(),
                        c.Type,
                        (ScalarExpression)CreateColumnReferenceExpression(c.Name.LastOrDefault())));
                case SelectScalarExpression scalar:
                    return new[]
                    {(
                        scalar.ColumnName?.Value ?? InferName(scalar.Expression),
                        InferType(scalar.Expression, table, scope),
                        scalar.Expression
                    )}.AsEnumerable();
                case SelectSetVariable set:
                    scope.Env.Vars[set.Variable.Name] =
                        Evaluate(
                            set.AssignmentKind,
                            scope.Env.Vars[set.Variable.Name],
                            Evaluate(set.Expression, NullArgument.It, scope));
                    return null; // TODO: what to return for projection column(s)?
                default:
                    throw FeatureNotSupportedException.Subtype(s);
            }
        };

        // TODO: Need to accumulate table aliases in Scope
        private static (Table, Scope) Join(
            Table accumulatedTables,
            Table targetTable,
            BooleanExpression condition,
            QualifiedJoinType type,
            Scope scope) => (
                new Table
                {
                    Columns = accumulatedTables.Columns.Concat(targetTable.Columns).ToList(),
                    Rows = QualifiedJoinRows(accumulatedTables, targetTable, condition, type, scope).ToList()
                },
                scope);

        private static IEnumerable<Row> QualifiedJoinRows(
            Table accumulatedTables,
            Table targetTable,
            BooleanExpression condition,
            QualifiedJoinType type,
            Scope scope) =>
            type switch
            {
                QualifiedJoinType.Inner => accumulatedTables.Rows.SelectMany(a =>
                    targetTable.Rows.Select(b => InnerRow(a, b))
                        .Where(c => Evaluate(condition, new RowArgument(c), scope))),
                QualifiedJoinType.LeftOuter => accumulatedTables.Rows.SelectMany(a =>
                    targetTable.Rows.Select(x => InnerRow(a, x))
                        .Where(c => Evaluate(condition, new RowArgument(c), scope))
                        .DefaultIfEmpty(LeftRow(a, targetTable))),
                QualifiedJoinType.RightOuter => targetTable.Rows.SelectMany(a =>
                    accumulatedTables.Rows.Select(x => InnerRow(a, x))
                        .Where(c => Evaluate(condition, new RowArgument(c), scope))
                        .DefaultIfEmpty(RightRow(accumulatedTables, a))),
                QualifiedJoinType.FullOuter => OuterRows(accumulatedTables, targetTable,
                    c => Evaluate(condition, new RowArgument(c), scope)),
                _ => throw FeatureNotSupportedException.Value(type)
            };

        private static (Table, Scope) Join(
            Table accumulatedTables,
            Table targetTable,
            UnqualifiedJoinType type,
            Scope scope) => (
                new Table
                {
                    Columns = accumulatedTables.Columns.Concat(targetTable.Columns).ToList(),
                    Rows = UnqualifiedJoinRows(accumulatedTables, targetTable, type).ToList()
                },
                scope);

        private static IEnumerable<Row> UnqualifiedJoinRows(
            Table accumulatedTables,
            Table targetTable,
            UnqualifiedJoinType type)
        {
            switch (type)
            {
                case UnqualifiedJoinType.CrossJoin:
                case UnqualifiedJoinType.CrossApply:
                    return accumulatedTables.Rows.SelectMany(a =>
                        targetTable.Rows
                            .Select(b => InnerRow(a, b)));
                case UnqualifiedJoinType.OuterApply:
                    return OuterRows(accumulatedTables, targetTable, _ => true);
                default:
                    throw FeatureNotSupportedException.Value(type);
            }
        }

        private static Row InnerRow(Table xs, Row x, Table ys, Row y, Scope scope) =>
            new Row
            {
                Columns = x.Columns.Concat(y.Columns).ToList(),
                Values = x.Values.Concat(y.Values).ToList(),
                Sources = new Dictionary<EquatableArray<string>, Row>
                {
                    {
                        EquatableArray.Of(scope.ExpandTableName(new [] {xs.Name})),
                        x
                    },
                    {
                        EquatableArray.Of(scope.ExpandTableName(new [] {ys.Name})),
                        y
                    }
                }
            };

        private static Row LeftRow(Table xs, Row x, Table ys, Scope scope) =>
            new Row
            {
                Columns = x.Columns.Concat(ys.Columns).ToList(),
                Values = x.Values.Concat(Nulls(ys.Columns.Count)).ToList(),
                Sources = new Dictionary<EquatableArray<string>, Row>
                {
                    {
                        EquatableArray.Of(scope.ExpandTableName(new [] {xs.Name})),
                        x
                    }
                }
            };

        private static Row RightRow(Table xs, Table ys, Row y, Scope scope) =>
            new Row
            {
                Columns = xs.Columns.Concat(y.Columns).ToList(),
                Values = Nulls(xs.Columns.Count).Concat(y.Values).ToList(),
                Sources = new Dictionary<EquatableArray<string>, Row>
                {
                    {
                        EquatableArray.Of(scope.ExpandTableName(new [] {ys.Name})),
                        y
                    }
                }
            };

        private static Row InnerRow(Row x, Row y) =>
            new Row
            {
                Columns = x.Columns.Concat(y.Columns).ToList(),
                Values = x.Values.Concat(y.Values).ToList()
            };

        private static Row LeftRow(Row x, Table ys) =>
            new Row
            {
                Columns = x.Columns.Concat(ys.Columns).ToList(),
                Values = x.Values.Concat(Nulls(ys.Columns.Count)).ToList()
            };

        private static Row RightRow(Table xs, Row y) =>
            new Row
            {
                Columns = xs.Columns.Concat(y.Columns).ToList(),
                Values = Nulls(xs.Columns.Count).Concat(y.Values).ToList()
            };

        private static IEnumerable<Row> OuterRows(
            Table xs,
            Table ys,
            Func<Row, bool> cond)
        {
            var outerRows = new List<Row>();
            var matchedRightRows = new HashSet<Row>();

            foreach (var a in xs.Rows)
            {
                var matchCount = 0;

                foreach (var b in ys.Rows)
                {
                    var c = InnerRow(a, b);

                    if (cond(c))
                    {
                        matchCount++;
                        outerRows.Add(c);
                        matchedRightRows.Add(b);
                    }
                }

                if (matchCount == 0)
                {
                    outerRows.Add(LeftRow(a, ys));
                }
            }

            outerRows.AddRange(ys.Rows.Where(b => !matchedRightRows.Contains(b)).Select(b => RightRow(xs, b)));
            return outerRows;
        }

        private static IEnumerable<object> Nulls(int n) => Enumerable.Repeat<object>(null, n);

        private static string InferName(GroupingSpecification groupSpec) =>
            groupSpec switch
            {
                ExpressionGroupingSpecification expr => InferName(expr.Expression),
                _ => null
            };

        private static string InferName(ScalarExpression expr) =>
            expr switch
            {
                ColumnReferenceExpression colRefExpr => colRefExpr.MultiPartIdentifier.Identifiers.Last().Value,
                VariableReference varRef => varRef.Name,
                _ => null
            };

        private static DbType InferType(ScalarExpression expr, Table table, Scope scope)
        {
            // TODO: a lot of work to do here for type inference
            //       how does sql server do it?
            //       is it just the lowest common type between all values in a column?
            //       do the columns not have type?

            switch (expr)
            {
                case ParenthesisExpression paren:
                    return InferType(paren.Expression, table, scope);
                case IntegerLiteral _:
                    return DbType.Int32;
                case StringLiteral _:
                    return DbType.String;
                // TODO: need to handle multi-table disambiguation
                case ColumnReferenceExpression colRefExpr:
                    var name = colRefExpr.MultiPartIdentifier.Identifiers.Last().Value;
                    return table.GetColumn(name).Type;
                case GlobalVariableExpression _:
                case VariableReference _:
                    return DbType.Object; // TODO: retain variable type information
                case BinaryExpression binExpr:
                    // TODO: so, so brittle
                    return InferType(binExpr.FirstExpression, table, scope);
                case FunctionCall fun:
                    switch (fun.FunctionName.Value.ToLower())
                    {
                        case "isnull":
                        case "sum":
                            return InferType(fun.Parameters[0], table, scope);
                        case "count":
                            return DbType.Int32;
                        case "trim":
                        case "ltrim":
                        case "rtrim":
                        case "upper":
                        case "lower":
                            return DbType.String;
                        case "dateadd":
                            return DbType.DateTime;
                        default: return scope.Env.Functions[fun.FunctionName.Value].ReturnType;
                    }
                case CaseExpression c:
                    return InferType(c.ElseExpression, table, scope);
                case IIfCall c:
                    return InferType(c.ElseExpression, table, scope);
                case NullIfExpression nullIf:
                    return InferType(nullIf.FirstExpression, table, scope);
                case NullLiteral _:
                    return DbType.String; // TODO: should there be a Null type?
                default:
                    throw FeatureNotSupportedException.Value(expr);
            }
        }

        private static Type TranslateType(DataTypeReference typeRef)
        {
            if (typeRef is SqlDataTypeReference type)
            {
                switch (type.SqlDataTypeOption)
                {
                    case SqlDataTypeOption.Bit:
                        return typeof(bool);
                    case SqlDataTypeOption.TinyInt:
                        return typeof(sbyte);
                    case SqlDataTypeOption.SmallInt:
                        return typeof(short);
                    case SqlDataTypeOption.Int:
                        return typeof(int);
                    case SqlDataTypeOption.BigInt:
                        return typeof(long);
                    case SqlDataTypeOption.Float:
                        return typeof(float);
                    case SqlDataTypeOption.Decimal:
                        return typeof(decimal);
                    case SqlDataTypeOption.DateTime:
                        return typeof(DateTime);
                    case SqlDataTypeOption.NText:
                    case SqlDataTypeOption.NVarChar:
                    case SqlDataTypeOption.Text:
                    case SqlDataTypeOption.VarChar:
                        return typeof(string);
                    case SqlDataTypeOption.Sql_Variant:
                        return typeof(object);
                    default:
                        throw FeatureNotSupportedException.Value(type.SqlDataTypeOption);
                }
            }

            throw FeatureNotSupportedException.Subtype(typeRef);
        }

        private static DbType TranslateDbType(DataTypeReference opt)
        {
            if (!(opt is SqlDataTypeReference)) throw FeatureNotSupportedException.Subtype(opt);

            var type = ((SqlDataTypeReference)opt).SqlDataTypeOption;

            switch (type)
            {
                case SqlDataTypeOption.TinyInt:
                case SqlDataTypeOption.SmallInt:
                   return DbType.Int16;
                case SqlDataTypeOption.Int:
                    return DbType.Int32;
                case SqlDataTypeOption.BigInt:
                    return DbType.Int64;
                case SqlDataTypeOption.Binary:
                    return DbType.Binary;
                case SqlDataTypeOption.Bit:
                    return DbType.Boolean;
                case SqlDataTypeOption.Date:
                    return DbType.Date;
                case SqlDataTypeOption.DateTime:
                    return DbType.DateTime;
                case SqlDataTypeOption.DateTime2:
                    return DbType.DateTime2;
                case SqlDataTypeOption.DateTimeOffset:
                    return DbType.DateTimeOffset;
                case SqlDataTypeOption.Cursor:
                case SqlDataTypeOption.Char:
                case SqlDataTypeOption.Text:
                case SqlDataTypeOption.VarChar:
                case SqlDataTypeOption.NChar:
                case SqlDataTypeOption.NText:
                case SqlDataTypeOption.NVarChar:
                    return DbType.AnsiString;
                case SqlDataTypeOption.Decimal:
                    return DbType.Decimal;
                default:
                    throw FeatureNotSupportedException.Value(type);
            }
        }

        public static Type TranslateCsType(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return typeof(string);
                case DbType.SByte:
                    return typeof(sbyte);
                case DbType.Int16:
                    return typeof(short);
                case DbType.Int32:
                    return typeof(int);
                case DbType.Int64:
                    return typeof(long);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return typeof(DateTime);
                case DbType.Decimal:
                    return typeof(decimal);
                case DbType.Boolean:
                    return typeof(bool);
                case DbType.Object:
                    return typeof(object);
                default:
                    throw FeatureNotSupportedException.Value(type);
            }
        }
    }
}
