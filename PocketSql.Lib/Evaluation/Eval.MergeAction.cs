using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        // TODO: return output if output clause is present
        public static void Evaluate(MergeAction action, DataTable targetTable, DataRow row, Env env)
        {
            switch (action)
            {
                case InsertMergeAction insert:
                    Evaluate(targetTable, insert.Columns, insert.Source, env);
                    break;
                case UpdateMergeAction update:
                    Evaluate(update.SetClauses, row, null, env);
                    break;
                case DeleteMergeAction _:
                    targetTable.Rows.Remove(row);
                    break;
            }
        }
    }
}
