using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static Table Evaluate(TableReference tableRef, Table joinedTables, Scope scope)
        {
            switch (tableRef)
            {
                case NamedTableReference named:
                    var name = named.SchemaObject.BaseIdentifier.Value;
                    return scope.Env.Views.IsDefined(name)
                        ? Evaluate(scope.Env.Views[name].Query, scope).ResultSet
                        : scope.Env.Tables[name];
                case DataModificationTableReference dml:
                    // TODO: how does this work?
                    return Evaluate(dml.DataModificationSpecification, scope).ResultSet;
                case JoinParenthesisTableReference paren:
                    return Evaluate(paren.Join, joinedTables, scope);
                case OdbcQualifiedJoinTableReference odbc:
                    return Evaluate(odbc.TableReference, joinedTables, scope);
                case QualifiedJoin qjoin:
                case UnqualifiedJoin ujoin:
                case JoinTableReference join:
                default:
                    throw FeatureNotSupportedException.Subtype(tableRef);
            }
        }
    }
}
