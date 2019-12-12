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

            return statement switch
            {
                SelectStatement select => Evaluate(select, scope),
                InsertStatement insert => Evaluate(insert.InsertSpecification, scope),
                UpdateStatement update => Evaluate(update.UpdateSpecification, scope),
                DeleteStatement delete => Evaluate(delete.DeleteSpecification, scope),
                MergeStatement merge => Evaluate(merge.MergeSpecification, scope),
                _ => throw FeatureNotSupportedException.Subtype(statement)
            };
        }
    }
}
