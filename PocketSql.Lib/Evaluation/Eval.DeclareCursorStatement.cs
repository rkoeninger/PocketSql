using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(DeclareCursorStatement declare, Scope scope)
        {
            var query = declare.CursorDefinition.Select.QueryExpression;
            var scroll = declare.CursorDefinition.Options.Any(x => x.OptionKind == CursorOptionKind.Scroll);
            scope.Env.Vars.Declare(declare.Name.Value, new Cursor(query, scroll));
        }
    }
}
