using System;
using System.Collections.Generic;
using System.Data;

namespace PocketSql
{
    public class Vars
    {
        public static Vars Of(IDataParameterCollection parameters) => new Vars().AddAll(parameters);

        private readonly IDictionary<string, object> vars = new Dictionary<string, object>();

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

        public Vars Declare(string name, object value)
        {
            vars.Add(name.TrimStart('@'), value);
            return this;
        }

        public bool IsDeclared(string name) => vars.ContainsKey(name.TrimStart('@'));

        public Vars AddAll(IDataParameterCollection parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                vars[parameter.ParameterName.TrimStart('@')] = parameter.Value;
            }

            return this;
        }
    }
}
