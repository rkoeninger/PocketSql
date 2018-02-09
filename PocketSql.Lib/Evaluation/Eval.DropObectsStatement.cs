using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static void Evaluate(DropObjectsStatement drop, Env env)
        {
            switch (drop)
            {
                case DropFunctionStatement _:
                    DropAll(drop, env.Functions);
                    return;
                case DropProcedureStatement _:
                    DropAll(drop, env.Procedures);
                    return;
                case DropTableStatement _:
                    DropAll(drop, env.Tables);
                    return;
                case DropViewStatement _:
                    DropAll(drop, env.Views);
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

                if (!drop.IsIfExists || ns.IsDefined(name))
                {
                    ns.Drop(name);
                }
            }
        }
    }
}
