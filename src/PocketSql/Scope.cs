using System.Collections.Immutable;
using System.Linq;
using PocketSql.Modeling;

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

        public string[] ExpandColumnName(string[] name)
        {
            var resolvedName = name
                .Take(name.Length - 1)
                .SelectMany(x =>
                    Aliases.TryGetValue(x, out var resolved)
                        ? resolved
                        : new[] { x })
                .Concat(new[] { name.Last() })
                .ToArray();

            if (resolvedName.Length < 3) resolvedName = Prefix(Env.DefaultSchema, resolvedName);
            if (resolvedName.Length < 4) resolvedName = Prefix(Env.DefaultDatabase, resolvedName);

            return resolvedName;
        }

        public string[] ExpandTableName(string[] name)
        {
            var resolvedName = name
                .SelectMany(x =>
                    Aliases.TryGetValue(x, out var resolved)
                        ? resolved
                        : new[] { x })
                .ToArray();

            if (resolvedName.Length < 2) resolvedName = Prefix(Env.DefaultSchema, resolvedName);
            if (resolvedName.Length < 3) resolvedName = Prefix(Env.DefaultDatabase, resolvedName);

            return resolvedName;
        }

        private static T[] Prefix<T>(T item, T[] array) =>
            new[] { item }.Concat(array).ToArray();
    }
}
