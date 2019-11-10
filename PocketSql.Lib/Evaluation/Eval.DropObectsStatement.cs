using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(DropObjectsStatement drop, Scope scope)
        {
            switch (drop)
            {
                case DropFunctionStatement _:
                    DropAll(drop, scope.Env.Functions);
                    return;
                case DropProcedureStatement _:
                    DropAll(drop, scope.Env.Procedures);
                    return;
                case DropTableStatement _:
                    DropAll(drop, scope.Env.Tables);
                    return;
                case DropViewStatement _:
                    DropAll(drop, scope.Env.Views);
                    return;
                default:
                    throw FeatureNotSupportedException.Subtype(drop);
            }
        }

        private static void DropAll<T>(DropObjectsStatement drop, Namespace<T> ns)
        {
            foreach (var x in drop.Objects)
            {
                var name = x.BaseIdentifier.Value;

                if (drop.IsIfExists)
                {
                    ns.DropIfExists(name);
                }
                else
                {
                    ns.Drop(name);
                }
            }
        }
    }
}
