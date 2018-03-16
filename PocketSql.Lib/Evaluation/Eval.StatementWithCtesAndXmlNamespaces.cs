using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(StatementWithCtesAndXmlNamespaces statement, Scope scope)
        {
            if (statement.WithCtesAndXmlNamespaces?.CommonTableExpressions != null)
            {
                scope = Evaluate(statement.WithCtesAndXmlNamespaces.CommonTableExpressions, scope);
            }

            switch (statement)
            {
                case SelectStatement select:
                    return Evaluate(select, scope);
                case InsertStatement insert:
                    return Evaluate(insert.InsertSpecification, scope);
                case UpdateStatement update:
                    return Evaluate(update.UpdateSpecification, scope);
                case DeleteStatement delete:
                    return Evaluate(delete.DeleteSpecification, scope);
                case MergeStatement merge:
                    return Evaluate(merge.MergeSpecification, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(statement);
            }
        }
    }
}
