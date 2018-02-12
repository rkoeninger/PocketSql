using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(DeclareCursorStatement declare, Env env)
        {
            var query = declare.CursorDefinition.Select.QueryExpression;
            var scroll = declare.CursorDefinition.Options.Any(x => x.OptionKind == CursorOptionKind.Scroll);
            env.Vars.Declare(declare.Name.Value, new Cursor(query, scroll));
        }
    }
}
