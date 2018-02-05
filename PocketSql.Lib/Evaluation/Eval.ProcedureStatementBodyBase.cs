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

                        if (!env.Engine.Procedures.ContainsKey(func.Name))
                        {
                            throw new Exception("function does not exist");
                        }

                        env.Engine.Functions[func.Name] = func;
                    }
                    return;
                case AlterProcedureStatement alterProc:
                    {
                        var proc = BuildProc(alterProc);

                        if (!env.Engine.Procedures.ContainsKey(proc.Name))
                        {
                            throw new Exception("procedure does not exist");
                        }

                        env.Engine.Procedures[proc.Name] = proc;
                    }
                    return;
                case CreateFunctionStatement createFunc:
                    {
                        var func = BuildFunc(createFunc);
                        env.Engine.Functions.Add(func.Name, func);
                    }
                    return;
                case CreateProcedureStatement createProc:
                    {
                        var proc = BuildProc(createProc);
                        env.Engine.Procedures.Add(proc.Name, proc);
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
                Statements = funcExpr.StatementList
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
