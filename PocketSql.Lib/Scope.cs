using PocketSql.Modeling;
using System.Collections.Immutable;

namespace PocketSql
{
    public class Scope
    {
        public Scope(Env env)
        {
            Env = env;
            Aliases = ImmutableDictionary.Create<string, string[]>(Naming.Comparer);
            Ctes = ImmutableDictionary.Create<string, Table>(Naming.Comparer);
        }

        private Scope(
            Env env,
            ImmutableDictionary<string, string[]> aliases,
            ImmutableDictionary<string, Table> ctes)
        {
            Env = env;
            Aliases = aliases;
            Ctes = ctes;
        }

        public Env Env { get; }
        public ImmutableDictionary<string, string[]> Aliases { get; }
        public ImmutableDictionary<string, Table> Ctes { get; }

        public Scope PushAlias(string alias, string[] fullName) =>
            new Scope(Env, Aliases.SetItem(alias, fullName), Ctes);

        public Scope PushCte(string name, Table table) =>
            new Scope(Env, Aliases, Ctes.SetItem(name, table));
    }
}
