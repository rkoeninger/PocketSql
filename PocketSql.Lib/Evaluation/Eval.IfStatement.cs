using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(IfStatement conditional, Env env) =>
            Evaluate(Evaluate(conditional.Predicate, null, env)
                ? conditional.ThenStatement
                : conditional.ElseStatement, env);
    }
}
