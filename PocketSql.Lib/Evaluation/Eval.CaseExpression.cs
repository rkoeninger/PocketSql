using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(CaseExpression expr, DataRow row, Env env)
        {
            switch (expr)
            {
                case SimpleCaseExpression simple:
                    var input = Evaluate(simple.InputExpression, row, env);

                    foreach (var clause in simple.WhenClauses)
                    {
                        if (input != null && input.Equals(Evaluate(clause.WhenExpression, row, env)))
                        {
                            return Evaluate(clause.ThenExpression, row, env);
                        }
                    }

                    break;
                case SearchedCaseExpression searched:
                    foreach (var clause in searched.WhenClauses)
                    {
                        if (Evaluate(clause.WhenExpression, row, env))
                        {
                            return Evaluate(clause.ThenExpression, row, env);
                        }
                    }

                    break;
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }

            return Evaluate(expr.ElseExpression, row, env);
        }

        public static object Evaluate(CaseExpression expr, IGrouping<EquatableList, DataRow> group, Env env)
        {
            switch (expr)
            {
                case SimpleCaseExpression simple:
                    var input = Evaluate(simple.InputExpression, group, env);

                    foreach (var clause in simple.WhenClauses)
                    {
                        if (input != null && input.Equals(Evaluate(clause.WhenExpression, group, env)))
                        {
                            return Evaluate(clause.ThenExpression, group, env);
                        }
                    }

                    break;
                case SearchedCaseExpression searched:
                    foreach (var clause in searched.WhenClauses)
                    {
                        if (Evaluate(clause.WhenExpression, group, env))
                        {
                            return Evaluate(clause.ThenExpression, group, env);
                        }
                    }

                    break;
                default:
                    throw FeatureNotSupportedException.Subtype(expr);
            }

            return Evaluate(expr.ElseExpression, group, env);
        }
    }
}
