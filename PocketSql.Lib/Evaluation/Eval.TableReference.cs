using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static (Table, Scope) Evaluate(TableReference tableRef, Table joinedTables, Scope scope)
        {
            switch (tableRef)
            {
                case NamedTableReference named:
                    var baseName = named.SchemaObject.BaseIdentifier.Value;
                    var schema = scope.Env.GetSchema(named.SchemaObject);
                    return (
                        schema.Views.GetMaybe(baseName)
                            .Select(v => Evaluate(v.Query, scope).ResultSet)
                            .OrElse(() => schema.Tables[baseName]),
                        named.Alias == null
                            ? scope
                            : scope.PushAlias(
                                named.Alias.Value,
                                named.SchemaObject.Identifiers.Select(x => x.Value).ToArray()));
                case DataModificationTableReference dml:
                    // TODO: how does this work?
                    return (Evaluate(dml.DataModificationSpecification, scope).ResultSet, scope);
                case JoinParenthesisTableReference paren:
                    return Evaluate(paren.Join, joinedTables, scope);
                case OdbcQualifiedJoinTableReference odbc:
                    return Evaluate(odbc.TableReference, joinedTables, scope);
                case QualifiedJoin qjoin:
                    return Join(joinedTables, null, qjoin.QualifiedJoinType, scope);
                case UnqualifiedJoin ujoin:
                    return Join(joinedTables, null, ujoin.UnqualifiedJoinType, scope);
                default:
                    throw FeatureNotSupportedException.Subtype(tableRef);
            }
        }
    }
}
