using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;
using static PocketSql.Modeling.Extensions;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: change result types of various Evaluate to only return what
        //       they will be able to return: rowCount, resultSet, void, etc.
        public static EngineResult Evaluate(TSqlStatement statement, Scope scope) =>
            statement switch
            {
                StatementWithCtesAndXmlNamespaces ctes => Evaluate(ctes, scope),
                TruncateTableStatement truncate => Evaluate(truncate, scope),
                CreateTableStatement createTable => Evaluate(createTable, scope),
                CreateViewStatement createView => Evaluate(createView, scope),
                ProcedureStatementBodyBase exec => VoidNull<EngineResult>(() => Evaluate(exec, scope)),
                DropObjectsStatement drop => VoidNull<EngineResult>(() => Evaluate(drop, scope)),
                SetVariableStatement set => Evaluate(set, scope),
                DeclareVariableStatement declare => Evaluate(declare, scope),
                IfStatement conditional => Evaluate(conditional, scope),
                WhileStatement loop => Evaluate(loop, scope),
                // TODO: maybe everything should return a list of results? (e.g. BeginEndBlockStatement)
                //       or at least all Evaluate(____Statement) methods
                // TODO: set @@error after each statement
                BeginEndBlockStatement block => Evaluate(block.StatementList, scope).LastOrDefault(),
                UseStatement use => VoidNull<EngineResult>(() => scope.Env.DefaultDatabase = use.DatabaseName.Value),
                ExecuteStatement exec => Evaluate(exec.ExecuteSpecification, scope),
                ReturnStatement ret => VoidNull<EngineResult>(() =>
                    scope.Env.ReturnValue = Evaluate(ret.Expression, NullArgument.It, scope)),
                DeclareCursorStatement declare => VoidNull<EngineResult>(() => Evaluate(declare, scope)),
                CursorStatement cur => VoidNull<EngineResult>(() => Evaluate(cur, scope)),
                _ => throw FeatureNotSupportedException.Subtype(statement)
            };
    }
}
