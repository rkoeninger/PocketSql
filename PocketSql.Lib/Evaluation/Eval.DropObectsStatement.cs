using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(DropObjectsStatement drop, Env env)
        {
            switch (drop)
            {
                case DropFunctionStatement _:
                    DropAll(drop, env.Engine.Functions);
                    return;
                case DropProcedureStatement _:
                    DropAll(drop, env.Engine.Procedures);
                    return;
                case DropTableStatement _:
                    DropAll(drop, env.Engine.Tables);
                    return;
            }

            throw new NotImplementedException();
        }

        private static void DropAll<T>(DropObjectsStatement drop, IDictionary<string, T> dict)
        {
            foreach (var x in drop.Objects)
            {
                var name = x.BaseIdentifier.Value;

                if (!drop.IsIfExists && !dict.ContainsKey(name))
                {
                    throw new Exception();
                }

                dict.Remove(name);
            }
        }
    }
}
