using System;
using System.Collections.Generic;

namespace PocketSql
{
    public class Namespace<T>
    {
        private readonly IDictionary<string, T> members =
            new Dictionary<string, T>(new Naming.EqualityComparer());

        public bool IsDefined(string name) => members.ContainsKey(name);

        public T this[string name]
        {
            get => Get(name);
            set => Set(name, value);
        }

        public T Get(string name)
        {
            if (!IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" not defined");
            return members[name];
        }

        public void Set(string name, T value)
        {
            if (!IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" not defined");
            members[name] = value;
        }

        public void Set<U>(U value) where U : T, INamed => Set(value.Name, value);

        public void Declare(string name, T value)
        {
            if (IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" already exists");
            members.Add(name, value);
        }

        public void Declare<U>(U value) where U : T, INamed => Declare(value.Name, value);

        public void DeclareOrSet(string name, T value) => members[name] = value;

        public void DeclareOrSet<U>(U value) where U : T, INamed => DeclareOrSet(value.Name, value);

        public void Drop(string name)
        {
            if (!IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" not defined");
            members.Remove(name);
        }

        public Namespace<T> Copy()
        {
            var ns = new Namespace<T>();

            foreach (var member in members)
            {
                ns.members.Add(member.Key, member.Value);
            }

            return ns;
        }
    }
}
