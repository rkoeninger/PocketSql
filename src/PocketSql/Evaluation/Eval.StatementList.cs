using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: set @@error after each statement
        public static List<EngineResult> Evaluate(StatementList statements, Scope scope) =>
            statements.Statements.Select(s => Evaluate(s, scope)).Where(r => r != null).ToList();
    }
}
