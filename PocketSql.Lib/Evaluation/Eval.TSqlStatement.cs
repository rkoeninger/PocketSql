using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: change result types of various Evaluate to only return what
        //       they will be able to return: rowCount, resultSet, void, etc.

        public static EngineResult Evaluate(TSqlStatement statement, Scope scope)
        {
            switch (statement)
            {
                case SelectStatement select:
                    return Evaluate(select, scope);
                case UpdateStatement update:
                    return Evaluate(update.UpdateSpecification, scope);
                case InsertStatement insert:
                    return Evaluate(insert.InsertSpecification, scope);
                case DeleteStatement delete:
                    return Evaluate(delete.DeleteSpecification, scope);
                case MergeStatement merge:
                    return Evaluate(merge.MergeSpecification, scope);
                case TruncateTableStatement truncate:
                    return Evaluate(truncate, scope);
                case CreateTableStatement createTable:
                    return Evaluate(createTable, scope);
                case CreateViewStatement createView:
                    return Evaluate(createView, scope);
                case ProcedureStatementBodyBase exec:
                    Evaluate(exec, scope);
                    return null;
                case DropObjectsStatement drop:
                    Evaluate(drop, scope);
                    return null;
                case SetVariableStatement set:
                    return Evaluate(set, scope);
                case DeclareVariableStatement declare:
                    return Evaluate(declare, scope);
                case IfStatement conditional:
                    return Evaluate(conditional, scope);
                case WhileStatement loop:
                    return Evaluate(loop, scope);
                case BeginEndBlockStatement block:
                    // TODO: maybe everything should return a list of results?
                    //       or at least all Evaluate(____Statement) methods
                    return Evaluate(block.StatementList, scope).LastOrDefault();
                case UseStatement use:
                    scope.Env.DefaultDatabase = use.DatabaseName.Value;
                    return null;
                case ExecuteStatement exec:
                    return Evaluate(exec.ExecuteSpecification, scope);
                case ReturnStatement ret:
                    scope.Env.ReturnValue = Evaluate(ret.Expression, NullArgument.It, scope);
                    return null;
                case DeclareCursorStatement declare:
                    Evaluate(declare, scope);
                    return null;
                case CursorStatement cur:
                    Evaluate(cur, scope);
                    return null;
                default:
                    throw FeatureNotSupportedException.Subtype(statement);
            }
        }
    }
}
