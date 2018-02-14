using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static Table Evaluate(TableReference tableRef, Table joinedTables, Env env)
        {
            switch (tableRef)
            {
                case NamedTableReference named:
                    var name = named.SchemaObject.BaseIdentifier.Value;
                    return env.Views.IsDefined(name)
                        ? Evaluate(env.Views[name].Query, env).ResultSet
                        : env.Tables[name];
                case DataModificationTableReference dml:
                    // TODO: how does this work?
                    return Evaluate(dml.DataModificationSpecification, env).ResultSet;
                case JoinParenthesisTableReference paren:
                    return Evaluate(paren.Join, joinedTables, env);
                case OdbcQualifiedJoinTableReference odbc:
                    return Evaluate(odbc.TableReference, joinedTables, env);
                case QualifiedJoin qjoin:
                case UnqualifiedJoin ujoin:
                case JoinTableReference join:
                default:
                    throw FeatureNotSupportedException.Subtype(tableRef);
            }
        }
    }
}
