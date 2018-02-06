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

        public IDictionary<string, object> Vars { get; set; } =
            new Dictionary<string, object>(new CaseInsensitivity.EqualityComparer());

        public Engine Engine { get; private set; }
        public string DefaultDatabase { get; set; } = "master";
        public string DefaultSchema { get; set; } = "dbo";

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
                if (!IsDeclared(name))
                {
                    throw new Exception($"Variable not declared: {name}");
                }

                return Vars[name];
            }
            set
            {
                if (!IsDeclared(name))
                {
                    throw new Exception($"Variable not declared: {name}");
                }

                Vars[name] = value;
            }
        }

        public Env Declare(string name, object value)
        {
            if (IsDeclared(name))
            {
                throw new Exception($"Variable already declared: {name}");
            }

            Vars.Add(name, value);
            return this;
        }

        public bool IsDeclared(string name) => Vars.ContainsKey(name);

        public Env AddAll(IDataParameterCollection parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                Vars[PrefixAt(parameter.ParameterName)] = parameter.Value;
            }

            return this;
        }

        private static string PrefixAt(string name) => name.StartsWith("@") ? name : $"@{name}";

        public Namespace<Function> Functions => Engine.Databases[DefaultDatabase].Schemas[DefaultSchema].Functions;
        public Namespace<Procedure> Procedures => Engine.Databases[DefaultDatabase].Schemas[DefaultSchema].Procedures;
        public Namespace<DataTable> Tables => Engine.Databases[DefaultDatabase].Schemas[DefaultSchema].Tables;
    }
}
