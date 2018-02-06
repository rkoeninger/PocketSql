using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PocketSql
{
    // TODO: have a base env that is just tables
    //       and a eval context env that has vars - Locals and Globals
    public class Env
    {
        public static Env Of(Engine engine, IDataParameterCollection parameters)
        {
            var env = new Env();
            env.AddAll(parameters);
            env.Engine = engine;
            return env;
        }

        public IDictionary<string, object> Vars { get; set; } = new Dictionary<string, object>();

        public Engine Engine { get; private set; }
        public string DefaultDatabase { get; set; }
        public string DefaultSchema { get; set; }

        public Env Fork() => new Env
        {
            Engine = Engine,
            DefaultDatabase = DefaultDatabase,
            DefaultSchema = DefaultSchema,
            Vars = Vars.ToDictionary(x => x.Key, x => x.Value)
        };

        public object this[string name]
        {
            get
            {
                var trimmedName = name.TrimStart('@');

                if (!Vars.ContainsKey(trimmedName))
                {
                    throw new Exception($"Variable not defined: {name}");
                }

                return Vars[trimmedName];
            }
            set
            {
                if (!IsDeclared(name))
                {
                    throw new Exception($"Variable not declared: {name}");
                }

                Vars[name.TrimStart('@')] = value;
            }
        }

        public Env Declare(string name, object value)
        {
            Vars.Add(name.TrimStart('@'), value);
            return this;
        }

        public bool IsDeclared(string name) => Vars.ContainsKey(name.TrimStart('@'));

        public Env AddAll(IDataParameterCollection parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                Vars[parameter.ParameterName.TrimStart('@')] = parameter.Value;
            }

            return this;
        }
    }
}
