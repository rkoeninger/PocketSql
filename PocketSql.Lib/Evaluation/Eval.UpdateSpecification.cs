using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(UpdateSpecification update, Env env)
        {
            var tableRef = (NamedTableReference)update.Target;
            var table = env.Engine.Tables[tableRef.SchemaObject.BaseIdentifier.Value];
            DataTable output = null;

            if (update.OutputClause != null)
            {
                // TODO: extract and share logic with Evaluate(SelectStatement, ...)
                // TODO: handle inserted.* vs deleted.* and $action
                var selections = update.OutputClause.SelectColumns.SelectMany(s =>
                {
                    switch (s)
                    {
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

                output = new DataTable();

                foreach (var (name, type, _) in selections)
                {
                    output.Columns.Add(new DataColumn
                    {
                        ColumnName = name,
                        DataType = type
                    });
                }
            }

            var rowCount = 0;

            foreach (DataRow row in table.Rows)
            {
                if (update.WhereClause == null || Evaluate(update.WhereClause.SearchCondition, row, env))
                {
                    Evaluate(update.SetClauses, row, output, env);
                    rowCount++;
                }
            }

            return new EngineResult(rowCount);
            // TODO: ResultSet = output
        }

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

        private static Type InferType(ScalarExpression expr, DataTable table)
        {
            // TODO: a lot of work to do here for type inference
            //       how does sql server do it?
            //       is it just the lowest common type between all values in a column?
            //       do the columns not have type?

            switch (expr)
            {
                // TODO: need to handle multi-table disambiguation
                case ColumnReferenceExpression colRefExpr:
                    var name = colRefExpr.MultiPartIdentifier.Identifiers.Last().Value;
                    return table.Columns[name].DataType;
                case VariableReference varRef:
                    return typeof(object); // TODO: retain variable type information
                case BinaryExpression binExpr:
                    // TODO: so, so brittle
                    return InferType(binExpr.FirstExpression, table);
                case FunctionCall fun:
                    switch (fun.FunctionName.Value.ToLower())
                    {
                        case "sum": return InferType(fun.Parameters[0], table);
                        case "count": return typeof(int);
                    }
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
