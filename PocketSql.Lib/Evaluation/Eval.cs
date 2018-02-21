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

        private static Func<SelectElement, IEnumerable<(string, DbType, ScalarExpression)>>
            ExtractSelection(Table table, Scope scope) => s =>
        {
            switch (s)
            {
                // TODO: respect table alias in star expression
                // TODO: function calls like count(*) are SelectStarExpressions
                case SelectStarExpression star:
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
                default:
                    throw FeatureNotSupportedException.Subtype(s);
            }
        };

        private static string InferName(GroupingSpecification groupSpec)
        {
            switch (groupSpec)
            {
                case ExpressionGroupingSpecification expr:
                    return InferName(expr.Expression);
            }

            return null;
        }

        private static string InferName(ScalarExpression expr)
        {
            switch (expr)
            {
                case ColumnReferenceExpression colRefExpr:
                    return colRefExpr.MultiPartIdentifier.Identifiers.Last().Value;
                case VariableReference varRef:
                    return varRef.Name;
            }

            return null;
        }

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
                        case "sum": return InferType(fun.Parameters[0], table, scope);
                        case "count": return DbType.Int32;
                        case "trim":
                        case "ltrim":
                        case "rtrim":
                        case "upper":
                        case "lower":
                            return DbType.String;
                        default: return scope.Env.Functions[fun.FunctionName.Value].ReturnType;
                    }
                case CaseExpression c:
                    return InferType(c.ElseExpression, table, scope);
                case IIfCall c:
                    return InferType(c.ElseExpression, table, scope);
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
                case SqlDataTypeOption.Int:
                    return DbType.Int32;
                case SqlDataTypeOption.BigInt:
                    return DbType.Int64;
                case SqlDataTypeOption.Binary:
                    return DbType.Binary;
                case SqlDataTypeOption.Bit:
                    return DbType.Boolean;
                case SqlDataTypeOption.Char:
                    return DbType.AnsiString;
                case SqlDataTypeOption.Date:
                    return DbType.Date;
                case SqlDataTypeOption.DateTime:
                    return DbType.DateTime;
                case SqlDataTypeOption.DateTime2:
                    return DbType.DateTime2;
                case SqlDataTypeOption.DateTimeOffset:
                    return DbType.DateTimeOffset;
                case SqlDataTypeOption.Cursor:
                case SqlDataTypeOption.VarChar:
                    return DbType.AnsiString;
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
                default:
                    throw FeatureNotSupportedException.Value(type);
            }
        }
    }
}
