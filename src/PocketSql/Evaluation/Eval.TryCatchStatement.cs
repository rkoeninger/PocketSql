using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(TryCatchStatement t, Scope scope)
        {
            try
            {
                return Evaluate(t.TryStatements, scope).LastOrDefault();
            }
            catch (SqlException e)
            {
                var previousErrorNumber = scope.Env.ErrorNumber;
                var previousErrorState = scope.Env.ErrorState;
                var previousErrorMessage = scope.Env.ErrorMessage;
                scope.Env.ErrorNumber = e.Number;
                scope.Env.ErrorState = e.State;
                scope.Env.ErrorMessage = e.Message;
                var result = Evaluate(t.CatchStatements, scope).LastOrDefault();
                scope.Env.ErrorNumber = previousErrorNumber;
                scope.Env.ErrorState = previousErrorState;
                scope.Env.ErrorMessage = previousErrorMessage;
                return result;
            }
        }
    }
}
