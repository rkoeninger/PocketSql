using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static List<EngineResult> Evaluate(TSqlFragment fragment, Scope scope)
        {
            return fragment switch
            {
                StatementList statements => Evaluate(statements, scope),
                TSqlStatement statement => new List<EngineResult> {Evaluate(statement, scope)}
                    .Where(r => r != null)
                    .ToList(),
                TSqlScript script => (
                    from b in script.Batches
                    from s in b.Statements
                    let x = Evaluate(s, scope)
                    where x != null
                    select x).ToList(),
                _ => throw FeatureNotSupportedException.Subtype(fragment)
            };
        }
    }
}
