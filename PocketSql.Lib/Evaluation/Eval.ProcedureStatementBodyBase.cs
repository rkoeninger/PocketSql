using System;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(ProcedureStatementBodyBase exec, Env env)
        {
            switch (exec)
            {
                case AlterFunctionStatement alterFunc:
                    {
                        var func = BuildFunc(alterFunc);
                        env.Functions[func.Name] = func;
                    }
                    return;
                case AlterProcedureStatement alterProc:
                    {
                        var proc = BuildProc(alterProc);
                        env.Procedures[proc.Name] = proc;
                    }
                    return;
                case CreateFunctionStatement createFunc:
                    {
                        var func = BuildFunc(createFunc);
                        env.Functions.Declare(func.Name, func);
                    }
                    return;
                case CreateProcedureStatement createProc:
                    {
                        var proc = BuildProc(createProc);
                        env.Procedures.Declare(proc.Name, proc);
                    }
                    return;
            }

            throw new NotSupportedException();
        }

        private static Function BuildFunc(FunctionStatementBody funcExpr) =>
            new Function
            {
                Name = funcExpr.Name.BaseIdentifier.Value,
                Parameters = funcExpr.Parameters.ToDictionary(
                    x => x.VariableName.Value,
                    x => TranslateType(x.DataType)),
                Statements = funcExpr.StatementList,
                ReturnType = TranslateType(((ScalarFunctionReturnType)funcExpr.ReturnType).DataType)
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
