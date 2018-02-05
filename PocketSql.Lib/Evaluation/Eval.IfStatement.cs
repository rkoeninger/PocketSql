﻿using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(IfStatement conditional, Env env) =>
            Evaluate(
                Evaluate(conditional.Predicate, (DataRow)null, env)
                    ? conditional.ThenStatement
                    : conditional.ElseStatement,
                env);
    }
}