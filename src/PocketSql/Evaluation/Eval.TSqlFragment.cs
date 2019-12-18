using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using static PocketSql.Modeling.Extensions;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static List<EngineResult> Evaluate(TSqlFragment fragment, Scope scope) =>
            fragment switch
            {
                StatementList statements => Evaluate(statements, scope),
                TSqlStatement statement => ListOf(Evaluate(statement, scope))
                    .Where(r => r != null)
                    .ToList(),
                TSqlScript script => script.Batches
                    .SelectMany(b => b.Statements)
                    .Select(s => Evaluate(s, scope))
                    .Where(x => x != null)
                    .ToList(),
                _ => throw FeatureNotSupportedException.Subtype(fragment)
            };
    }
}
