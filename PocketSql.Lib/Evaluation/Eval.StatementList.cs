using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static List<EngineResult> Evaluate(StatementList statements, Env env) =>
            statements.Statements.Select(s => Evaluate(s, env)).Where(r => r != null).ToList();
    }
}
