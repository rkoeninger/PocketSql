using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: need to handle different Drop___Statement differently

        public static EngineResult Evaluate(DropObjectsStatement drop, Env env)
        {
            foreach (var table in drop.Objects)
            {
                var tableName = table.BaseIdentifier.Value;

                if (!drop.IsIfExists && !env.Engine.tables.ContainsKey(tableName))
                {
                    throw new Exception();
                }

                env.Engine.tables.Remove(tableName);
            }

            return null;
        }
    }
}
