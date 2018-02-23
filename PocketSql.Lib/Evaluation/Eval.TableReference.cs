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
                    var baseName = named.SchemaObject.BaseIdentifier.Value;
                    var schema = scope.Env.GetSchema(named.SchemaObject);
                    return schema.Views.GetMaybe(baseName)
                        .Select(v => Evaluate(v.Query, scope).ResultSet)
                        .OrElse(() => schema.Tables[baseName]);
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
