using System;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: change result types of various Evaluate to only return what
        //       they will be able to return: rowCount, resultSet, void, etc.

        public static EngineResult Evaluate(TSqlStatement statement, Env env)
        {
            switch (statement)
            {
                case SelectStatement select:
                    return Evaluate(select, env);
                case UpdateStatement update:
                    return Evaluate(update, env);
                case InsertStatement insert:
                    return Evaluate(insert, env);
                case DeleteStatement delete:
                    return Evaluate(delete, env);
                case MergeStatement merge:
                    return Evaluate(merge, env);
                case TruncateTableStatement truncate:
                    return Evaluate(truncate, env);
                case CreateTableStatement createTable:
                    return Evaluate(createTable, env);
                case DropObjectsStatement drop:
                    return Evaluate(drop, env);
                case SetVariableStatement set:
                    return Evaluate(set, env);
                case DeclareVariableStatement declare:
                    return Evaluate(declare, env);
                case IfStatement conditional:
                    return Evaluate(conditional, env);
                case WhileStatement loop:
                    return Evaluate(loop, env);
                case BeginEndBlockStatement block:
                    // TODO: maybe everything should return a list of results?
                    //       or at least all Evaluate(____Statement) methods
                    return Evaluate(block.StatementList, env).LastOrDefault();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
