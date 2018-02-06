using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(ScalarExpression expr, DataRow row, Env env)
        {
            switch (expr)
            {
                case IntegerLiteral intLiteral:
                    return int.Parse(intLiteral.Value);
                case NumericLiteral numericExpr:
                    return decimal.Parse(numericExpr.Value);
                case StringLiteral stringExpr:
                    return stringExpr.Value;
                case UnaryExpression unaryExpr:
                    return Evaluate(
                        unaryExpr.UnaryExpressionType,
                        Evaluate(unaryExpr.Expression, row, env));
                case BinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, row, env),
                        Evaluate(binaryExpr.SecondExpression, row, env));
                case ColumnReferenceExpression colExpr:
                    return row[colExpr.MultiPartIdentifier.Identifiers.Last().Value];
                case VariableReference varRef:
                    return env.Vars[varRef.Name];
                case CaseExpression caseExpr:
                    return Evaluate(caseExpr, row, env);
                case FunctionCall funCall:
                    return Evaluate(funCall, row, env);
            }

            throw new NotImplementedException();
        }

        public static object Evaluate(ScalarExpression expr, IGrouping<EquatableList, DataRow> group, Env env)
        {
            switch (expr)
            {
                case ColumnReferenceExpression colExpr:
                    // TODO: need to know column names for EquatableList
                    // TODO: this only works when there is only 1 column grouped by
                    return group.Key.Elements.First().Item2;
                case FunctionCall funCall:
                    return Evaluate(funCall, group, env);
            }

            throw new NotImplementedException();
        }
    }
}
