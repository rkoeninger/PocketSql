using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

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

        private static Func<SelectElement, IEnumerable<(string, Type, ScalarExpression)>>
            ExtractSelection(DataTable table, Env env) => s =>
        {
            switch (s)
            {
                // TODO: respect table alias in star expression
                case SelectStarExpression star:
                    return table.Columns.Cast<DataColumn>().Select(c => (
                        c.ColumnName,
                        c.DataType,
                        (ScalarExpression)CreateColumnReferenceExpression(c.ColumnName)));
                case SelectScalarExpression scalar:
                    return new[]
                    {(
                        scalar.ColumnName?.Value ?? InferName(scalar.Expression),
                        InferType(scalar.Expression, table, env),
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

        private static Type InferType(ScalarExpression expr, DataTable table, Env env)
        {
            // TODO: a lot of work to do here for type inference
            //       how does sql server do it?
            //       is it just the lowest common type between all values in a column?
            //       do the columns not have type?

            switch (expr)
            {
                case ParenthesisExpression paren:
                    return InferType(paren.Expression, table, env);
                case IntegerLiteral _:
                    return typeof(int);
                case StringLiteral _:
                    return typeof(string);
                // TODO: need to handle multi-table disambiguation
                case ColumnReferenceExpression colRefExpr:
                    var name = colRefExpr.MultiPartIdentifier.Identifiers.Last().Value;
                    return table.Columns[name].DataType;
                case GlobalVariableExpression _:
                case VariableReference _:
                    return typeof(object); // TODO: retain variable type information
                case BinaryExpression binExpr:
                    // TODO: so, so brittle
                    return InferType(binExpr.FirstExpression, table, env);
                case FunctionCall fun:
                    switch (fun.FunctionName.Value.ToLower())
                    {
                        case "sum": return InferType(fun.Parameters[0], table, env);
                        case "count": return typeof(int);
                        case "trim":
                        case "ltrim":
                        case "rtrim":
                        case "upper":
                        case "lower":
                            return typeof(string);
                        default: return env.Functions[fun.FunctionName.Value].ReturnType;
                    }
                case CaseExpression c:
                    return InferType(c.ElseExpression, table, env);
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
    }
}
