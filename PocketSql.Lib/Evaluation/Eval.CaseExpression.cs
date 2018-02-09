using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(CaseExpression caseExpr, DataRow row, Env env)
        {
            switch (caseExpr)
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
                    throw new NotImplementedException();
            }

            return Evaluate(caseExpr.ElseExpression, row, env);
        }

        public static object Evaluate(CaseExpression caseExpr, IGrouping<EquatableList, DataRow> group, Env env)
        {
            switch (caseExpr)
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
                    throw new NotImplementedException();
            }

            return Evaluate(caseExpr.ElseExpression, group, env);
        }
    }
}
