using System;
using System.Collections.Generic;
using System.Data;

namespace PocketSql
{
    public class Env
    {
        public static Env Of(Engine engine, IDataParameterCollection parameters)
        {
            var env = new Env();
            env.AddAll(parameters);
            env.Engine = engine;
            return env;
        }

        private readonly IDictionary<string, object> vars = new Dictionary<string, object>();

        public Engine Engine { get; private set; }

        public object this[string name]
        {
            get => vars[name.TrimStart('@')];
            set
            {
                if (!IsDeclared(name))
                {
                    throw new Exception($"Variable not declared: {name}");
                }

                vars[name.TrimStart('@')] = value;
            }
        }

        public Env Declare(string name, object value)
        {
            vars.Add(name.TrimStart('@'), value);
            return this;
        }

        public bool IsDeclared(string name) => vars.ContainsKey(name.TrimStart('@'));

        public Env AddAll(IDataParameterCollection parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                vars[parameter.ParameterName.TrimStart('@')] = parameter.Value;
            }

            return this;
        }
    }
}
