using System.Data;

namespace PocketSql
{
    public class Env
    {
        public static Env Of(EngineConnection connection, IDataParameterCollection parameters)
        {
            var env = new Env();
            env.AddAll(parameters);
            env.Engine = connection.Engine;
            env.DefaultDatabase = connection.Database;
            return env;
        }

        public Namespace<object> Vars { get; private set; } = new Namespace<object>();
        public object ReturnValue { get; set; }
        public Engine Engine { get; private set; }
        public string DefaultDatabase { get; set; } = "master";
        public string DefaultSchema { get; set; } = "dbo";

        public Env Fork() => new Env
        {
            Engine = Engine,
            DefaultDatabase = DefaultDatabase,
            DefaultSchema = DefaultSchema,
            Vars = Vars.Copy()
        };

        public void AddAll(IDataParameterCollection parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                Vars.Declare(Naming.Parameter(parameter.ParameterName), parameter.Value);
            }
        }

        public Namespace<Function> Functions => Engine.Databases[DefaultDatabase].Schemas[DefaultSchema].Functions;
        public Namespace<Procedure> Procedures => Engine.Databases[DefaultDatabase].Schemas[DefaultSchema].Procedures;
        public Namespace<DataTable> Tables => Engine.Databases[DefaultDatabase].Schemas[DefaultSchema].Tables;
    }
}
