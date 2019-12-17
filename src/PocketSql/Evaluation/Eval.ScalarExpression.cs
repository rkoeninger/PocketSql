using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static T Evaluate<T>(ScalarExpression expr, IArgument arg, Scope scope) =>
            Evaluate(expr, arg, scope).As<T>();

        public static object Evaluate(ScalarExpression expr, IArgument arg, Scope scope) =>
            expr switch
            {
                ParenthesisExpression paren => Evaluate(paren.Expression, arg, scope),
                IntegerLiteral intLiteral => int.Parse(intLiteral.Value),
                NumericLiteral numericExpr => decimal.Parse(numericExpr.Value),
                StringLiteral stringExpr => stringExpr.Value,
                UnaryExpression unaryExpr => Evaluate(unaryExpr.UnaryExpressionType,
                    Evaluate(unaryExpr.Expression, arg, scope)),
                BinaryExpression binaryExpr => Evaluate(binaryExpr.BinaryExpressionType,
                    Evaluate(binaryExpr.FirstExpression, arg, scope),
                    Evaluate(binaryExpr.SecondExpression, arg, scope)),
                ColumnReferenceExpression colExpr => arg switch
                {
                    RowArgument row => row.Value.GetValue(
                        colExpr.MultiPartIdentifier.Identifiers.Select(x => x.Value).ToArray(), scope),
                    GroupArgument group => group.Key.Elements.First(x =>
                            x.Item1.Similar(colExpr.MultiPartIdentifier.Identifiers.Last().Value))
                        .Item2,
                    _ => throw FeatureNotSupportedException.Subtype(arg)
                },
                VariableReference varRef => scope.Env.Vars[varRef.Name],
                GlobalVariableExpression globRef => scope.Env.GetGlobal(globRef.Name),
                CaseExpression caseExpr => Evaluate(caseExpr, arg, scope),
                IIfCall iif => Evaluate(iif, arg, scope),
                FunctionCall funCall => Evaluate(funCall, arg, scope),
                NullLiteral _ => null,
                NullIfExpression nullIf => Evaluate(nullIf, arg, scope),
                _ => throw FeatureNotSupportedException.Subtype(expr)
            };
    }
}
