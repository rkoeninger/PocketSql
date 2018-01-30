using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(InsertStatement insert, Env env)
        {
            var namedTableRef = (NamedTableReference)insert.InsertSpecification.Target;
            var table = env.Engine.tables[namedTableRef.SchemaObject.BaseIdentifier.Value];

            switch (insert.InsertSpecification.InsertSource)
            {
                case ValuesInsertSource values:
                    return Evaluate(
                        table,
                        insert.InsertSpecification.Columns,
                        values,
                        env);
                case SelectInsertSource select:
                    return Evaluate(
                        table,
                        insert.InsertSpecification.Columns,
                        Evaluate(select.Select, env).ResultSet,
                        env);
            }

            throw new NotImplementedException();
        }
    }
}
