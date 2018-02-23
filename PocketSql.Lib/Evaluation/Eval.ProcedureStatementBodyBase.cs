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
                    scope.Env.GetSchema(alterFunc.Name).Functions.Set(
                        alterFunc.Name.BaseIdentifier.Value,
                        BuildFunc(alterFunc));
                    return;
                case AlterProcedureStatement alterProc:
                    scope.Env.GetSchema(alterProc.ProcedureReference.Name).Procedures.Set(
                        alterProc.ProcedureReference.Name.BaseIdentifier.Value,
                        BuildProc(alterProc));
                    return;
                case CreateFunctionStatement createFunc:
                    scope.Env.GetSchema(createFunc.Name).Functions.Declare(
                        createFunc.Name.BaseIdentifier.Value,
                        BuildFunc(createFunc));
                    return;
                case CreateProcedureStatement createProc:
                    scope.Env.GetSchema(createProc.ProcedureReference.Name).Procedures.Declare(
                        createProc.ProcedureReference.Name.BaseIdentifier.Value,
                        BuildProc(createProc));
                    return;
                default:
                    throw FeatureNotSupportedException.Subtype(exec);
            }
        }

        private static Function BuildFunc(FunctionStatementBody funcExpr) =>
            new Function
            {
                Name = new[]
                {
                    funcExpr.Name.DatabaseIdentifier?.Value,
                    funcExpr.Name.SchemaIdentifier?.Value,
                    funcExpr.Name.BaseIdentifier?.Value
                }.Where(x => x != null).ToArray(),
                Parameters = funcExpr.Parameters.ToDictionary(
                    x => x.VariableName.Value,
                    x => TranslateDbType(x.DataType)),
                Statements = funcExpr.StatementList,
                ReturnType = TranslateDbType(((ScalarFunctionReturnType)funcExpr.ReturnType).DataType)
            };

        private static Procedure BuildProc(ProcedureStatementBody procExpr) =>
            new Procedure
            {
                Name = new[]
                {
                    procExpr.ProcedureReference.Name.DatabaseIdentifier?.Value,
                    procExpr.ProcedureReference.Name.SchemaIdentifier?.Value,
                    procExpr.ProcedureReference.Name.BaseIdentifier?.Value
                }.Where(x => x != null).ToArray(),
                Parameters = procExpr.Parameters.ToDictionary(
                    x => x.VariableName.Value,
                    x => TranslateType(x.DataType)),
                Statements = procExpr.StatementList
            };
    }
}
