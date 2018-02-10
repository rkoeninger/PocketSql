using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(CaseExpression expr, IArgument arg, Env env)
        {
            switch (expr)
            {
                case SimpleCaseExpression simple:
                    var input = Evaluate(simple.InputExpression, arg, env);

                    foreach (var clause in simple.WhenClauses)
                    {
                        if (input != null && input.Equals(Evaluate(clause.WhenExpression, arg, env)))
                        {
                            return Evaluate(clause.ThenExpression, arg, env);
                        }
                    }

                    break;
                case SearchedCaseExpression searched:
                    foreach (var clause in searched.WhenClauses)
                    {
                        if (Evaluate(clause.WhenExpression, arg, env))
                        {
                            return Evaluate(clause.ThenExpression, arg, env);
                        }
                    }

                    break;
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }

            return Evaluate(expr.ElseExpression, arg, env);
        }
    }
}
