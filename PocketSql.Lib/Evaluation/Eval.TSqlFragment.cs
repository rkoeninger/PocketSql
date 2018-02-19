using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static List<EngineResult> Evaluate(TSqlFragment fragment, Scope scope)
        {
            switch (fragment)
            {
                case StatementList statements:
                    return Evaluate(statements, scope);
                case TSqlStatement statement:
                    var r = Evaluate(statement, scope);
                    return r != null ? new List<EngineResult> { r } : new List<EngineResult>();
                case TSqlScript script:
                    return (
                        from b in script.Batches
                        from s in b.Statements
                        let x = Evaluate(s, scope)
                        where x != null
                        select x
                    ).ToList();
                default:
                    throw FeatureNotSupportedException.Subtype(fragment);
            }
        }
    }
}
