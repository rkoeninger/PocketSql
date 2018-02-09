using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(ScalarExpression expr, Env env) =>
            Evaluate(expr, (DataRow) null, env);

        public static object Evaluate(ScalarExpression expr, DataRow row, Env env)
        {
            switch (expr)
            {
                case ParenthesisExpression paren:
                    return Evaluate(paren.Expression, row, env);
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
                case GlobalVariableExpression globRef:
                    return env.GetGlobal(globRef.Name);
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
                case IntegerLiteral intLiteral:
                    return int.Parse(intLiteral.Value);
                case NumericLiteral numericExpr:
                    return decimal.Parse(numericExpr.Value);
                case StringLiteral stringExpr:
                    return stringExpr.Value;
                case UnaryExpression unaryExpr:
                    return Evaluate(
                        unaryExpr.UnaryExpressionType,
                        Evaluate(unaryExpr.Expression, group, env));
                case BinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, group, env),
                        Evaluate(binaryExpr.SecondExpression, group, env));
                case ColumnReferenceExpression colExpr:
                    return group.Key.Elements.First(x => x.Item1.Similar(colExpr.MultiPartIdentifier.Identifiers.Last().Value)).Item2;
                case VariableReference varRef:
                    return env.Vars[varRef.Name];
                case GlobalVariableExpression globRef:
                    return env.GetGlobal(globRef.Name);
                case CaseExpression caseExpr:
                    return Evaluate(caseExpr, group, env);
                case FunctionCall funCall:
                    return Evaluate(funCall, group, env);
            }

            throw new NotImplementedException();
        }
    }
}
