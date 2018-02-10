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
                    return Evaluate(update.UpdateSpecification, env);
                case InsertStatement insert:
                    return Evaluate(insert.InsertSpecification, env);
                case DeleteStatement delete:
                    return Evaluate(delete.DeleteSpecification, env);
                case MergeStatement merge:
                    return Evaluate(merge.MergeSpecification, env);
                case TruncateTableStatement truncate:
                    return Evaluate(truncate, env);
                case CreateTableStatement createTable:
                    return Evaluate(createTable, env);
                case CreateViewStatement createView:
                    return Evaluate(createView, env);
                case ProcedureStatementBodyBase exec:
                    Evaluate(exec, env);
                    return null;
                case DropObjectsStatement drop:
                    Evaluate(drop, env);
                    return null;
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
                case UseStatement use:
                    env.DefaultDatabase = use.DatabaseName.Value;
                    return null;
                case ExecuteStatement exec:
                    return Evaluate(exec.ExecuteSpecification, env);
                case ReturnStatement ret:
                    env.ReturnValue = Evaluate(ret.Expression, NullArgument.It, env);
                    return null;
                case DeclareCursorStatement declare:
                    Evaluate(declare, env);
                    return null;
                case CursorStatement cur:
                    Evaluate(cur, env);
                    return null;
                default:
                    throw FeatureNotSupportedException.Subtype(statement);
            }
        }
    }
}
