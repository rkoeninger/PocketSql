using System;
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
                                scope.ExpandTableName(named.SchemaObject.Identifiers.Select(x => x.Value).ToArray())));
                case DataModificationTableReference dml:
                    // TODO: how does this work?
                    return (Evaluate(dml.DataModificationSpecification, scope).ResultSet, scope);
                case JoinParenthesisTableReference paren:
                    return Evaluate(paren.Join, joinedTables, scope);
                case OdbcQualifiedJoinTableReference odbc:
                    return Evaluate(odbc.TableReference, joinedTables, scope);
                case QueryDerivedTable query:
                    var table = Evaluate(query.QueryExpression, scope).ResultSet;
                    table.Name = Guid.NewGuid().ToString().Replace("-", "");
                    return (table, scope.PushAlias(query.Alias.Value, new[] { table.Name }));
                case QualifiedJoin qjoin:
                    var (leftTable, leftScope) = Evaluate(qjoin.FirstTableReference, joinedTables, scope);
                    var (rightTable, rightScope) = Evaluate(qjoin.SecondTableReference, leftTable, leftScope);
                    return Join(leftTable, rightTable, qjoin.SearchCondition, qjoin.QualifiedJoinType, rightScope);
                case UnqualifiedJoin ujoin:
                    var (leftTable2, leftScope2) = Evaluate(ujoin.FirstTableReference, joinedTables, scope);
                    var (rightTable2, rightScope2) = Evaluate(ujoin.SecondTableReference, leftTable2, leftScope2);
                    return Join(leftTable2, rightTable2, ujoin.UnqualifiedJoinType, rightScope2);
                default:
                    throw FeatureNotSupportedException.Subtype(tableRef);
            }
        }
    }
}
