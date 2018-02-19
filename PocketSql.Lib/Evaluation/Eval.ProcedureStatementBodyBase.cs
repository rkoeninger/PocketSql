using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(ProcedureStatementBodyBase exec, Scope scope)
        {
            switch (exec)
            {
                case AlterFunctionStatement alterFunc:
                    scope.Env.Functions.Set(BuildFunc(alterFunc));
                    return;
                case AlterProcedureStatement alterProc:
                    scope.Env.Procedures.Set(BuildProc(alterProc));
                    return;
                case CreateFunctionStatement createFunc:
                    scope.Env.Functions.Declare(BuildFunc(createFunc));
                    return;
                case CreateProcedureStatement createProc:
                    scope.Env.Procedures.Declare(BuildProc(createProc));
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
