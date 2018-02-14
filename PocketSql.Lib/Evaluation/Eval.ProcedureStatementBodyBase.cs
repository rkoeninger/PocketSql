using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(ProcedureStatementBodyBase exec, Env env)
        {
            switch (exec)
            {
                case AlterFunctionStatement alterFunc:
                    env.Functions.Set(BuildFunc(alterFunc));
                    return;
                case AlterProcedureStatement alterProc:
                    env.Procedures.Set(BuildProc(alterProc));
                    return;
                case CreateFunctionStatement createFunc:
                    env.Functions.Declare(BuildFunc(createFunc));
                    return;
                case CreateProcedureStatement createProc:
                    env.Procedures.Declare(BuildProc(createProc));
                    return;
                default:
                    throw FeatureNotSupportedException.Subtype(exec);
            }
        }

        private static Function BuildFunc(FunctionStatementBody funcExpr) =>
            new Function
            {
                Name = funcExpr.Name.BaseIdentifier.Value,
                Parameters = funcExpr.Parameters.ToDictionary(
                    x => x.VariableName.Value,
                    x => TranslateDbType(x.DataType)),
                Statements = funcExpr.StatementList,
                ReturnType = TranslateDbType(((ScalarFunctionReturnType)funcExpr.ReturnType).DataType)
            };

        private static Procedure BuildProc(ProcedureStatementBody procExpr) =>
            new Procedure
            {
                Name = procExpr.ProcedureReference.Name.Identifiers.Last().Value,
                Parameters = procExpr.Parameters.ToDictionary(
                    x => x.VariableName.Value,
                    x => TranslateType(x.DataType)),
                Statements = procExpr.StatementList
            };
    }
}
