using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(DataModificationSpecification dml, Scope scope)
        {
            switch (dml)
            {
                case InsertSpecification insert:
                    return Evaluate(insert, scope);
                case MergeSpecification merge:
                    return Evaluate(merge, scope);
                case DeleteSpecification delete:
                    return Evaluate(delete, scope);
                case UpdateSpecification update:
                    return Evaluate(update, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(dml);
            }
        }
    }
}
