using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static List<EngineResult> Evaluate(TSqlFragment fragment, Env env)
        {
            switch (fragment)
            {
                case StatementList statements:
                    return Evaluate(statements, env);
                case TSqlStatement statement:
                    var r = Evaluate(statement, env);
                    return r != null ? new List<EngineResult> { r } : new List<EngineResult>();
                case TSqlScript script:
                    return (
                        from b in script.Batches
                        from s in b.Statements
                        let x = Evaluate(s, env)
                        where x != null
                        select x
                    ).ToList();
                default:
                    throw FeatureNotSupportedException.Subtype(fragment);
            }
        }
    }
}
