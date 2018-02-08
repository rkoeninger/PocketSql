using System;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DataModificationSpecification dml, Env env)
        {
            switch (dml)
            {
                case InsertSpecification insert:
                    return Evaluate(insert, env);
                case MergeSpecification merge:
                    return Evaluate(merge, env);
                case DeleteSpecification delete:
                    return Evaluate(delete, env);
                case UpdateSpecification update:
                    return Evaluate(update, env);
            }

            throw new NotImplementedException();
        }
    }
}
