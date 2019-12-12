using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(InsertSpecification insert, IOutputSink sink, Scope scope)
        {
            var table = scope.Env.GetTable((NamedTableReference)insert.Target);

            return insert.InsertSource switch
            {
                ValuesInsertSource values => Evaluate(table, insert.Columns, values, NullArgument.It, sink, scope),
                SelectInsertSource select => Evaluate(table, insert.Columns, Evaluate(@select.Select, scope).ResultSet,
                    sink, scope),
                _ => throw FeatureNotSupportedException.Subtype(insert.InsertSource)
            };
        }
    }
}
