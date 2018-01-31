using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static DataTable Evaluate(TableReference tableRef, DataTable joinedTables, Env env)
        {
            switch (tableRef)
            {
                case NamedTableReference named:
                    return env.Engine.tables[named.SchemaObject.BaseIdentifier.Value];
                case DataModificationTableReference dml:
                    // TODO: how does this work?
                    return Evaluate(dml.DataModificationSpecification, env).ResultSet;
                case JoinParenthesisTableReference paren:
                    break;
                case JoinTableReference join:
                    break;
                case OdbcQualifiedJoinTableReference odbc:
                    return Evaluate(odbc.TableReference, joinedTables, env);
            }

            throw new NotImplementedException();
        }
    }
}
