using System;
using System.Collections;
using System.Collections.Generic;

namespace PocketSql.Modeling
{
    public class Namespace<T> : IEnumerable<T>
    {
        private readonly IDictionary<string, T> members = new Dictionary<string, T>(Naming.Comparer);

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

        public Maybe<T> GetMaybe(string name) =>
            members.TryGetValue(name, out var value) ? Maybe.Some(value) : Maybe.None<T>();

        public T GetOrAdd(string name, Func<string, T> make)
        {
            if (members.TryGetValue(name, out var result))
            {
                return result;
            }

            var value = make(name);
            members.Add(name, value);
            return value;
        }

        public void Set(string name, T value)
        {
            if (!IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" not defined");
            members[name] = value;
        }

        public void Declare(string name, T value)
        {
            if (IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" already exists");
            members.Add(name, value);
        }

        public void Declare<U>(U value) where U : T, INamed => Declare(value.Name, value);

        public void DeclareOrSet(string name, T value) => members[name] = value;

        public void Drop(string name)
        {
            if (!IsDefined(name)) throw new Exception($"{typeof(T).Name} \"{name}\" not defined");
            members.Remove(name);
        }

        public void DropIfExists(string name) => members.Remove(name);

        public Namespace<T> Copy()
        {
            var ns = new Namespace<T>();

            foreach (var member in members)
            {
                ns.members.Add(member.Key, member.Value);
            }

            return ns;
        }

        public IEnumerator<T> GetEnumerator() => members.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
