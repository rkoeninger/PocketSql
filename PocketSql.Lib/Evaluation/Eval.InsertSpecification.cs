using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(InsertSpecification insert, Scope scope)
        {
            var namedTableRef = (NamedTableReference)insert.Target;
            var table = scope.Env.Tables[namedTableRef.SchemaObject.BaseIdentifier.Value];
            var sink = new NullOutputSink();

            switch (insert.InsertSource)
            {
                case ValuesInsertSource values:
                    return Evaluate(table, insert.Columns, values, NullArgument.It, sink, scope);
                case SelectInsertSource select:
                    return Evaluate(table, insert.Columns, Evaluate(select.Select, scope).ResultSet, sink, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(insert.InsertSource);
            }
        }
    }
}
