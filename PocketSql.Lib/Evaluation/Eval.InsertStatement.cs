using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(InsertStatement insert, Env env)
        {
            var namedTableRef = (NamedTableReference)insert.InsertSpecification.Target;
            var table = env.Engine.tables[namedTableRef.SchemaObject.BaseIdentifier.Value];
            return Evaluate(
                table,
                insert.InsertSpecification.Columns,
                (ValuesInsertSource)insert.InsertSpecification.InsertSource,
                env);
        }
    }
}
